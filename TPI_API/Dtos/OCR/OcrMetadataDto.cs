namespace TPI_API.Dtos.OCR;

public class OcrMetadataDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public string ProcessingTime { get; set; } = string.Empty;
    public OcrStatisticsDto Statistics { get; set; } = new OcrStatisticsDto();
}
