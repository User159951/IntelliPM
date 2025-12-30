using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Agents.Commands;

public class StoreNoteCommandHandler : IRequestHandler<StoreNoteCommand, StoreNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;

    public StoreNoteCommandHandler(IUnitOfWork unitOfWork, ILlmClient llmClient, IVectorStore vectorStore)
    {
        _unitOfWork = unitOfWork;
        _llmClient = llmClient;
        _vectorStore = vectorStore;
    }

    public async Task<StoreNoteResponse> Handle(StoreNoteCommand request, CancellationToken cancellationToken)
    {
        // Generate embedding
        var embedding = await _llmClient.GenerateEmbeddingAsync(request.Content, cancellationToken);

        // Store in pgvector
        await _vectorStore.StoreDocumentAsync(
            request.ProjectId,
            request.Type,
            request.Content,
            embedding,
            null,
            cancellationToken);

        // Also store in SQL Server for reference
        var doc = new DocumentStore
        {
            ProjectId = request.ProjectId,
            Type = request.Type,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var repo = _unitOfWork.Repository<DocumentStore>();
        await repo.AddAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StoreNoteResponse(doc.Id, true);
    }
}

