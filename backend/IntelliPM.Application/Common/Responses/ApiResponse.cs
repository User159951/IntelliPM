namespace IntelliPM.Application.Common.Responses;

public record ApiResponse<T>(bool Success, T Data, string Message = "", List<string>? Errors = null);

public record ApiResponse(bool Success, string Message = "", List<string>? Errors = null);

