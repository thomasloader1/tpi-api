using Microsoft.AspNetCore.Http;

namespace TPI_API.Services;

public interface IOcrService
{
    /// <summary>
    /// Procesa un archivo PDF utilizando OCR para extraer texto
    /// </summary>
    /// <param name="file">Archivo PDF a procesar</param>
    /// <returns>Resultado del procesamiento OCR con texto extraído y metadatos</returns>
    Task<OcrResult> ProcessPdfAsync(IFormFile file);

    /// <summary>
    /// Obtiene la ruta al directorio tessdata que contiene los datos de entrenamiento
    /// </summary>
    /// <returns>Ruta al directorio tessdata o null si no se encuentra</returns>
    string? GetTessdataPath();
}

/// <summary>
/// Modelo de resultado del procesamiento OCR
/// </summary>
public class OcrResult
{
    public string Status { get; set; } = "éxito";
    public string Message { get; set; } = "PDF procesado correctamente con Tesseract OCR.";
    public OcrMetadata Metadata { get; set; } = new OcrMetadata();
    public List<OcrPageResult> Pages { get; set; } = new List<OcrPageResult>();
    public string FullText { get; set; } = string.Empty;
}

public class OcrMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public string ProcessingTime { get; set; } = string.Empty;
    public OcrStatistics Statistics { get; set; } = new OcrStatistics();
}

public class OcrStatistics
{
    public int TotalCharacters { get; set; }
    public int TotalWords { get; set; }
    public double AverageCharactersPerPage { get; set; }
    public double AverageWordsPerPage { get; set; }
}

public class OcrPageResult
{
    public int Number { get; set; }
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Characters { get; set; }
    public int Words { get; set; }
}