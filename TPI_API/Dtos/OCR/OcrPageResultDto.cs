namespace TPI_API.Dtos.OCR;

public class OcrPageResultDto
{
    public int Number { get; set; }
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Characters { get; set; }
    public int Words { get; set; }
}
