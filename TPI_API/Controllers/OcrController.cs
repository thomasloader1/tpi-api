using Microsoft.AspNetCore.Mvc;
using TPI_API.Services;

namespace TPI_API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;

    public OcrController(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPdf([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se ha proporcionado un archivo PDF.");

        try
        {
            // Delegar el procesamiento al servicio de OCR
            var result = await _ocrService.ProcessPdfAsync(file);

            // Transformar el resultado a la respuesta esperada
            return Ok(new
            {
                estado = result.Status,
                mensaje = result.Message,
                metadatos = new
                {
                    nombreArchivo = result.Metadata.FileName,
                    tamaño = result.Metadata.FileSize,
                    totalPáginas = result.Metadata.TotalPages,
                    tiempoProcesamiento = result.Metadata.ProcessingTime,
                    estadísticas = new
                    {
                        totalCaracteres = result.Metadata.Statistics.TotalCharacters,
                        totalPalabras = result.Metadata.Statistics.TotalWords,
                        promedioCaracteresPorPágina = result.Metadata.Statistics.AverageCharactersPerPage,
                        promedioPalabrasPorPágina = result.Metadata.Statistics.AverageWordsPerPage
                    }
                },
                páginas = result.Pages.Select(p => new
                {
                    numero = p.Number,
                    texto = p.Text,
                    confianza = p.Confidence,
                    caracteres = p.Characters,
                    palabras = p.Words
                }).ToList(),
                textoCompleto = result.FullText
            });
        }
        catch (DirectoryNotFoundException ex)
        {
            return StatusCode(500, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al procesar OCR: {ex.Message}");
        }
    }
}
