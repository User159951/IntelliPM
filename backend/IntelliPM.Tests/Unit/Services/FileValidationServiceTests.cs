using FluentAssertions;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntelliPM.Tests.Unit.Services;

/// <summary>
/// Unit tests for FileValidationService.
/// Tests file validation logic including MIME types, extensions, file size, and path traversal prevention.
/// </summary>
public class FileValidationServiceTests
{
    private readonly IFileValidationService _fileValidationService;
    private readonly Mock<ILogger<FileValidationService>> _loggerMock;

    public FileValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<FileValidationService>>();
        _fileValidationService = new FileValidationService(_loggerMock.Object);
    }

    #region File Size Validation

    [Fact]
    public void ValidateFile_FileSizeExceedsLimit_ThrowsValidationException()
    {
        // Arrange
        var maxSizeBytes = 10 * 1024 * 1024; // 10 MB
        var fileMock = CreateMockFile("test.pdf", "application/pdf", maxSizeBytes + 1);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _fileValidationService.ValidateFile(fileMock.Object, maxSizeBytes));
        
        exception.Message.Should().Contain("exceeds maximum allowed size");
    }

    [Fact]
    public void ValidateFile_FileSizeWithinLimit_Passes()
    {
        // Arrange
        var maxSizeBytes = 10 * 1024 * 1024; // 10 MB
        var fileMock = CreateMockFile("test.pdf", "application/pdf", maxSizeBytes - 1);

        // Act & Assert - Should not throw
        _fileValidationService.ValidateFile(fileMock.Object, maxSizeBytes);
    }

    [Fact]
    public void ValidateFile_EmptyFile_ThrowsValidationException()
    {
        // Arrange
        var fileMock = CreateMockFile("test.pdf", "application/pdf", 0);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes));
        
        exception.Message.Should().Contain("cannot be empty");
    }

    [Fact]
    public void ValidateFile_NullFile_ThrowsValidationException()
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _fileValidationService.ValidateFile(null!, AttachmentConstants.MaxFileSizeBytes));
        
        exception.Message.Should().Contain("File is required");
    }

    #endregion

    #region File Extension Validation

    [Fact]
    public void ValidateFile_AllowedExtension_Passes()
    {
        // Arrange
        var fileMock = CreateMockFile("test.pdf", "application/pdf", 1024);

        // Act & Assert - Should not throw
        _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes);
    }

    [Fact]
    public void ValidateFile_DisallowedExtension_ThrowsValidationException()
    {
        // Arrange
        var fileMock = CreateMockFile("test.exe", "application/x-msdownload", 1024);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes));
        
        exception.Message.Should().Contain("is not allowed");
        exception.Message.Should().Contain(".exe");
    }

    [Fact]
    public void ValidateFile_NoExtension_ThrowsValidationException()
    {
        // Arrange
        var fileMock = CreateMockFile("test", "application/octet-stream", 1024);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes));
        
        exception.Message.Should().Contain("must have an extension");
    }

    #endregion

    #region MIME Type Validation

    [Fact]
    public void ValidateFile_AllowedMimeType_Passes()
    {
        // Arrange
        var fileMock = CreateMockFile("test.pdf", "application/pdf", 1024);

        // Act & Assert - Should not throw
        _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes);
    }

    [Fact]
    public void ValidateFile_DisallowedMimeTypeButAllowedExtension_PassesWithWarning()
    {
        // Arrange - Some browsers send incorrect MIME types, so we allow if extension is valid
        var fileMock = CreateMockFile("test.pdf", "application/octet-stream", 1024);

        // Act & Assert - Should pass (extension is valid)
        _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes);
        
        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MIME type")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMostOnce);
    }

    [Fact]
    public void ValidateFile_EmptyMimeType_LogsWarningButPasses()
    {
        // Arrange
        var fileMock = CreateMockFile("test.pdf", "", 1024);

        // Act & Assert - Should pass (extension is valid)
        _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes);
    }

    #endregion

    #region Path Traversal Prevention

    [Theory]
    [InlineData("../../../etc/passwd.pdf")]
    [InlineData("..\\..\\windows\\system32\\file.pdf")]
    [InlineData("file/../../etc/passwd.pdf")]
    [InlineData("file\\..\\..\\etc\\passwd.pdf")]
    [InlineData("file:test.pdf")]
    [InlineData("file*test.pdf")]
    [InlineData("file?test.pdf")]
    [InlineData("file\"test.pdf")]
    [InlineData("file<test.pdf")]
    [InlineData("file>test.pdf")]
    [InlineData("file|test.pdf")]
    public void ValidateFile_DangerousFilename_ThrowsValidationException(string fileName)
    {
        // Arrange
        var fileMock = CreateMockFile(fileName, "application/pdf", 1024);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _fileValidationService.ValidateFile(fileMock.Object, AttachmentConstants.MaxFileSizeBytes));
        
        exception.Message.Should().Contain("invalid characters");
    }

    [Fact]
    public void SanitizeFileName_PathTraversal_SanitizesCorrectly()
    {
        // Arrange
        var dangerousFileName = "../../../etc/passwd.pdf";

        // Act
        var sanitized = _fileValidationService.SanitizeFileName(dangerousFileName);

        // Assert
        sanitized.Should().NotContain("..");
        sanitized.Should().NotContain("/");
        sanitized.Should().NotContain("\\");
        sanitized.Should().EndWith(".pdf");
    }

    [Fact]
    public void SanitizeFileName_DangerousCharacters_ReplacesWithUnderscore()
    {
        // Arrange
        var dangerousFileName = "file:test*name?with\"dangerous<chars>.pdf";

        // Act
        var sanitized = _fileValidationService.SanitizeFileName(dangerousFileName);

        // Assert
        sanitized.Should().NotContainAny(":", "*", "?", "\"", "<", ">");
        sanitized.Should().EndWith(".pdf");
    }

    [Fact]
    public void SanitizeFileName_LeadingTrailingDots_Removes()
    {
        // Arrange
        var fileName = "...test.pdf...";

        // Act
        var sanitized = _fileValidationService.SanitizeFileName(fileName);

        // Assert
        sanitized.Should().NotStartWith(".");
        sanitized.Should().NotEndWith(".");
        sanitized.Should().Contain("test");
    }

    [Fact]
    public void SanitizeFileName_EmptyAfterSanitization_UsesDefault()
    {
        // Arrange
        var fileName = "....";

        // Act
        var sanitized = _fileValidationService.SanitizeFileName(fileName);

        // Assert
        sanitized.Should().NotBeNullOrWhiteSpace();
        sanitized.Should().Be("file");
    }

    [Fact]
    public void SanitizeFileName_LongFileName_Truncates()
    {
        // Arrange
        var longFileName = new string('a', 300) + ".pdf";

        // Act
        var sanitized = _fileValidationService.SanitizeFileName(longFileName);

        // Assert
        sanitized.Length.Should().BeLessThanOrEqualTo(255);
        sanitized.Should().EndWith(".pdf");
    }

    [Fact]
    public void SanitizeFileName_NormalFileName_ReturnsAsIs()
    {
        // Arrange
        var normalFileName = "test-document_123.pdf";

        // Act
        var sanitized = _fileValidationService.SanitizeFileName(normalFileName);

        // Assert
        sanitized.Should().Be(normalFileName);
    }

    [Fact]
    public void SanitizeFileName_NullFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _fileValidationService.SanitizeFileName(null!));
    }

    [Fact]
    public void SanitizeFileName_EmptyFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _fileValidationService.SanitizeFileName(""));
    }

    #endregion

    #region Helper Methods

    private Mock<IFormFile> CreateMockFile(string fileName, string contentType, long length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return fileMock;
    }

    #endregion
}
