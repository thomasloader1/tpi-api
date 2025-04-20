namespace TPI_API.Dtos.DocumentProcessed;

public class ProcessedDocumentDto
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public List<ProcessedPageDto> ProcessedPages { get; set; } = new List<ProcessedPageDto>();
}
