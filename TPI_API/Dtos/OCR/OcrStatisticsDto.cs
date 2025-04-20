namespace TPI_API.Dtos.OCR;

public class OcrStatisticsDto
{
    public int TotalCharacters { get; set; }
    public int TotalWords { get; set; }
    public double AverageCharactersPerPage { get; set; }
    public double AverageWordsPerPage { get; set; }
}
