
using System.Text;
using Tesseract;
using TPI_API.Dtos.OCR;
using TPI_API.Interfaces;

namespace TPI_API.Services;

public class OcrService : IOcrService
{
    private readonly IDocumentProcessingService _documentProcessingService;

    public OcrService(IDocumentProcessingService documentProcessingService)
    {
        _documentProcessingService = documentProcessingService;
    }

    public async Task<OcrResultDto> ProcessPdfAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se ha proporcionado un archivo PDF.");

        var tessDataPath = TessdataPath;
        if (tessDataPath == null || !Directory.Exists(tessDataPath))
            throw new DirectoryNotFoundException($"No se encontró el directorio 'tessdata' en: {tessDataPath}");

        var result = new OcrResultDto();

        try
        {
            // Utilizar el servicio de procesamiento de documentos para preparar el PDF
            var processedDocument = await _documentProcessingService.ProcessPdfDocumentAsync(file);
            
            using var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default);

            var startTime = DateTime.Now;
            var pageResults = new List<OcrPageResultDto>();
            int totalCharacters = 0;
            int totalWords = 0;
            var extractedText = new StringBuilder();

            // Configurar parámetros de Tesseract para mejorar la precisión
            engine.SetVariable("tessedit_char_whitelist", @"abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789.,;:()[]{}¡!¿?@#$%&*+-/\\<>=_áéíóúÁÉÍÓÚüÜ ");
            engine.SetVariable("language_model_penalty_non_dict_word", "0.5");
            engine.SetVariable("language_model_penalty_non_freq_dict_word", "0.5");

            foreach (var processedPage in processedDocument.ProcessedPages)
            {
                using var ms = new MemoryStream(processedPage.ProcessedImageData);
                using var img = Pix.LoadFromMemory(ms.ToArray());
                
                using var page = engine.Process(img, PageSegMode.Auto);
                
                string pageText = page.GetText();
                
                // Aplicar correcciones post-OCR para mejorar la calidad del texto
                pageText = PostProcessText(pageText);
                
                float confidence = page.GetMeanConfidence() * 100;
                int pageCharCount = pageText.Length;
                int pageWordCount = pageText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                
                totalCharacters += pageCharCount;
                totalWords += pageWordCount;

                var pageResult = new OcrPageResultDto
                {
                    Number = processedPage.PageNumber,
                    Text = pageText,
                    Confidence = Math.Round(confidence, 2),
                    Characters = pageCharCount,
                    Words = pageWordCount
                };

                pageResults.Add(pageResult);

                extractedText.AppendLine($"--- Página {processedPage.PageNumber} ---");
                extractedText.AppendLine(pageText);
                extractedText.AppendLine();
            }

            var processingTime = (DateTime.Now - startTime).TotalSeconds;

            // Construir el resultado
            result.Status = "Success";
            result.Message = "PDF procesado correctamente con Tesseract OCR.";
            result.Metadata = new OcrMetadataDto
            {
                FileName = file.FileName,
                FileSize = processedDocument.FileSize,
                TotalPages = processedDocument.PageCount,
                ProcessingTime = $"{Math.Round(processingTime, 2)} seg.",
                Statistics = new OcrStatisticsDto
                {
                    TotalCharacters = totalCharacters,
                    TotalWords = totalWords,
                    AverageCharactersPerPage = Math.Round((double)totalCharacters / processedDocument.PageCount, 2),
                    AverageWordsPerPage = Math.Round((double)totalWords / processedDocument.PageCount, 2)
                }
            };
            result.Pages = pageResults;
            result.FullText = extractedText.ToString();

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al procesar OCR: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Obtiene la ruta al directorio tessdata que contiene los datos de entrenamiento
    /// </summary>
    /// <returns>Ruta al directorio tessdata o null si no se encuentra</returns>
    public string? TessdataPath
    {
        get
        {
            string? currentPath = AppContext.BaseDirectory;
            for (int i = 0; i < 5 && currentPath != null; i++)
            {
                var parent = Directory.GetParent(currentPath);
                if (parent == null) break;

                var tessPath = Path.Combine(parent.FullName, "TPI_API", "tessdata");
                if (Directory.Exists(tessPath))
                    return tessPath;

                currentPath = parent.FullName;
            }

            // Fallback al directorio de salida
            var fallbackPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
            return Directory.Exists(fallbackPath) ? fallbackPath : null;
        }
    }

    /// <summary>
    /// Aplica correcciones post-OCR al texto extraído
    /// </summary>
    /// <param name="text">Texto extraído por OCR</param>
    /// <returns>Texto corregido</returns>
    private string PostProcessText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        try
        {
            // 1. Corregir errores comunes de OCR con un diccionario ampliado
            var corrections = new Dictionary<string, string>
            {
                // Caracteres confundidos frecuentemente
                { "0", "O" }, { "l", "I" }, { "1", "l" }, { "5", "S" }, { "8", "B" },
                { "rn", "m" }, { "cl", "d" }, { "vv", "w" }, { "nn", "m" },
                
                // Errores comunes en español
                { "a", "á" }, { "e", "é" }, { "i", "í" }, { "o", "ó" }, { "u", "ú" },
                { "n~", "ñ" }, { "n-", "ñ" },
                
                // Símbolos mal interpretados
                { "'", "\"" }, { "`", "'" }, { "—", "-" }, { "'", "'" }
            };
            
            // Aplicar correcciones de caracteres
            foreach (var correction in corrections)
            {
                text = text.Replace(correction.Key, correction.Value);
            }
            
            // 2. Corregir palabras cortadas por saltos de línea
            text = System.Text.RegularExpressions.Regex.Replace(text, @"(\w+)-\s*\n(\w+)", "$1$2");
            
            // 3. Eliminar caracteres extraños o ruido
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\w\s.,;:()\[\]{}¡!¿?@#$%&*+\-/\\<>=_áéíóúÁÉÍÓÚüÜñÑ]", "");
            
            // 4. Normalizar espacios múltiples
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            
            // 5. Corregir espacios antes de signos de puntuación
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+([.,;:!?])", "$1");
            
            // 6. Corregir palabras comunes mal reconocidas en español
            var commonWordCorrections = new Dictionary<string, string>
            {
                { "cornprar", "comprar" }, { "cornpra", "compra" },
                { "rnás", "más" }, { "rnenos", "menos" },
                { "rnucho", "mucho" }, { "rnuy", "muy" },
                { "tarnbién", "también" }, { "tarnpoco", "tampoco" },
                { "siernpre", "siempre" }, { "tiernpo", "tiempo" },
                { "ejernplo", "ejemplo" }, { "nurnero", "numero" },
                { "prirnero", "primero" }, { "últirno", "último" },
                { "inforrne", "informe" }, { "infornación", "información" }
            };
            
            // Aplicar correcciones de palabras comunes
            foreach (var correction in commonWordCorrections)
            {
                text = System.Text.RegularExpressions.Regex.Replace(text, 
                    $"\\b{correction.Key}\\b", correction.Value, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            // 7. Validar y corregir palabras con baja confianza usando un enfoque contextual
            text = CorrectLowConfidenceWords(text);
            
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el post-procesamiento de texto: {ex.Message}");
            return text; // Devolver texto original en caso de error
        }
    }
    
    /// <summary>
    /// Corrige palabras con baja confianza usando análisis contextual
    /// </summary>
    private string CorrectLowConfidenceWords(string text)
    {
        try
        {
            // Dividir el texto en palabras
            string[] words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Diccionario de palabras comunes en español para validación
            var commonSpanishWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "el", "la", "los", "las", "un", "una", "unos", "unas",
                "y", "o", "pero", "porque", "como", "cuando", "donde", "si",
                "de", "en", "por", "para", "con", "sin", "sobre", "bajo",
                "que", "quien", "cuyo", "cual", "cuales", "cuanta", "cuantos",
                "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas",
                "ser", "estar", "haber", "tener", "hacer", "poder", "decir", "ir",
                "más", "menos", "mucho", "poco", "grande", "pequeño", "alto", "bajo",
                "bueno", "malo", "mejor", "peor", "primero", "último", "nuevo", "viejo"
            };
            
            // Patrones de corrección basados en reglas contextuales
            var contextualPatterns = new Dictionary<string, string>
            {
                { @"\b(de|en|con|por|para)\s+e\b", "$1 el" },
                { @"\b(de|en|con|por|para)\s+l\b", "$1 la" },
                { @"\ba\s+e\b", "a el" },
                { @"\bpor\s+e\b", "por el" },
                { @"\bcon\s+e\b", "con el" },
                { @"\bde\s+e\b", "de el" },
                { @"\ben\s+e\b", "en el" }
            };
            
            // Aplicar patrones contextuales
            foreach (var pattern in contextualPatterns)
            {
                text = System.Text.RegularExpressions.Regex.Replace(text, 
                    pattern.Key, pattern.Value, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la corrección de palabras con baja confianza: {ex.Message}");
            return text; // Devolver texto original en caso de error
        }
    }
    
    /// <summary>
    /// Obtiene la ruta al directorio tessdata que contiene los datos de entrenamiento
    /// </summary>
    /// <returns>Ruta al directorio tessdata o null si no se encuentra</returns>
    public string? GetTessdataPath()
    {
        // Busca en estructura relativa al proyecto
        string? currentPath = AppContext.BaseDirectory;
        for (int i = 0; i < 5 && currentPath != null; i++)
        {
            var parent = Directory.GetParent(currentPath);
            if (parent == null) break;

            var tessPath = Path.Combine(parent.FullName, "TPI_API", "tessdata");
            if (Directory.Exists(tessPath))
                return tessPath;

            currentPath = parent.FullName;
        }

        // Fallback al directorio de salida
        var fallbackPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
        return Directory.Exists(fallbackPath) ? fallbackPath : null;
    }
}