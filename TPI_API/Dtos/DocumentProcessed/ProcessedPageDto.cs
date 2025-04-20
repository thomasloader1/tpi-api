namespace TPI_API.Dtos.DocumentProcessed;

public class ProcessedPageDto
{
    public int PageNumber { get; set; }
    public byte[] ProcessedImageData { get; set; } = Array.Empty<byte>();
    public int Width { get; set; }
    public int Height { get; set; }
}
