using TPI_API.Dtos.OCR;

namespace TPI_API.Interfaces;

public interface IOcrService
{
    Task<OcrResultDto> ProcessPdfAsync(IFormFile file);
    string? TessdataPath { get; }
}
