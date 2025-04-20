using TPI_API.Dtos.DocumentProcessed;

namespace TPI_API.Interfaces;

public interface IDocumentProcessingService
{
    Task<ProcessedDocumentDto> ProcessPdfDocumentAsync(IFormFile file);
    Task<ProcessedDocumentDto> ProcessImageDocumentAsync(IFormFile file);
}
