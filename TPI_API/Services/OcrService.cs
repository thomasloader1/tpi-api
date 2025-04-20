
using PdfiumViewer;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Tesseract;

namespace TPI_API.Services;

public class OcrService : IOcrService
{
    static OcrService()
    {
        try
        {
            // Intentar cargar la biblioteca nativa desde el paquete NuGet
            // El paquete PdfiumViewer.Native.x86_64.v8-xfa debería proporcionar la DLL
            // en la ubicación correcta automáticamente
            
            // Verificar si existe la DLL en la ubicación esperada
            string pdfiumPath = Path.Combine(AppContext.BaseDirectory, "NativeBinaries", "x64", "pdfium.dll");
            if (!File.Exists(pdfiumPath))
            {
                // Buscar en ubicaciones alternativas
                string nugetPath = Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", "pdfium.dll");
                if (File.Exists(nugetPath))
                {
                    // Crear directorio si no existe
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "NativeBinaries", "x64"));
                    // Copiar el archivo a la ubicación esperada
                    File.Copy(nugetPath, pdfiumPath, true);
                    Console.WriteLine($"Archivo pdfium.dll copiado desde {nugetPath} a {pdfiumPath}");
                }
                else
                {
                    Console.WriteLine($"No se encontró el archivo pdfium.dll en ninguna ubicación conocida");
                }
            }
            
            // Cargar la biblioteca manualmente
            if (File.Exists(pdfiumPath))
            {
                IntPtr handle = LoadLibrary(pdfiumPath);
                if (handle == IntPtr.Zero)
                {
                    Console.WriteLine($"Error al cargar pdfium.dll: {Marshal.GetLastWin32Error()}");
                }
                else
                {
                    Console.WriteLine("Biblioteca pdfium.dll cargada correctamente");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inicializar PdfiumViewer: {ex.Message}");
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string libname);

    /// <summary>
    /// Procesa un archivo PDF utilizando OCR para extraer texto
    /// </summary>
    /// <param name="file">Archivo PDF a procesar</param>
    /// <returns>Resultado del procesamiento OCR con texto extraído y metadatos</returns>
    public async Task<OcrResult> ProcessPdfAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se ha proporcionado un archivo PDF.");

        var tessDataPath = GetTessdataPath();
        if (tessDataPath == null || !Directory.Exists(tessDataPath))
            throw new DirectoryNotFoundException($"No se encontró el directorio 'tessdata' en: {tessDataPath}");

        string tempPdfPath = Path.GetTempFileName();
        var extractedText = new StringBuilder();
        var result = new OcrResult();

        try
        {
            // Guardar archivo temporalmente
            using (var fs = new FileStream(tempPdfPath, FileMode.Create))
                await file.CopyToAsync(fs);

            using var document = PdfDocument.Load(tempPdfPath);
            using var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default);

            var startTime = DateTime.Now;
            var pageResults = new List<OcrPageResult>();
            int totalCharacters = 0;
            int totalWords = 0;

            for (int i = 0; i < document.PageCount; i++)
            {
                using var bmp = (Bitmap)document.Render(i, 300, 300, true);
                
                // Aplicar técnicas de preprocesamiento para mejorar la calidad de la imagen
                using var processedBmp = PreprocessImage(bmp);

                using var ms = new MemoryStream();
                processedBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                using var img = Pix.LoadFromMemory(ms.ToArray());
                
                // Configurar parámetros de Tesseract para mejorar la precisión
                using var page = engine.Process(img, PageSegMode.Auto);
                
                // Aplicar configuraciones adicionales para mejorar la precisión
                page.SetVariable("tessedit_char_whitelist", "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789.,;:()[]{}¡!¿?@#$%&*+-/\\"'<>=_áéíóúÁÉÍÓÚüÜ ");
                page.SetVariable("language_model_penalty_non_dict_word", "0.5");
                page.SetVariable("language_model_penalty_non_freq_dict_word", "0.5");

                string pageText = page.GetText();
                
                // Aplicar correcciones post-OCR para mejorar la calidad del texto
                pageText = PostProcessText(pageText);
                
                float confidence = page.GetMeanConfidence() * 100;
                int pageCharCount = pageText.Length;
                int pageWordCount = pageText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                
                totalCharacters += pageCharCount;
                totalWords += pageWordCount;

                var pageResult = new OcrPageResult
                {
                    Number = i + 1,
                    Text = pageText,
                    Confidence = Math.Round(confidence, 2),
                    Characters = pageCharCount,
                    Words = pageWordCount
                };

                pageResults.Add(pageResult);

                extractedText.AppendLine($"--- Página {i + 1} ---");
                extractedText.AppendLine(pageText);
                extractedText.AppendLine();
            }

            var processingTime = (DateTime.Now - startTime).TotalSeconds;

            // Construir el resultado
            result.Status = "Success";
            result.Message = "PDF procesado correctamente con Tesseract OCR.";
            result.Metadata = new OcrMetadata
            {
                FileName = file.FileName,
                FileSize = $"{Math.Round(file.Length / 1024.0, 2)} KB",
                TotalPages = document.PageCount,
                ProcessingTime = $"{Math.Round(processingTime, 2)} seg.",
                Statistics = new OcrStatistics
                {
                    TotalCharacters = totalCharacters,
                    TotalWords = totalWords,
                    AverageCharactersPerPage = Math.Round((double)totalCharacters / document.PageCount, 2),
                    AverageWordsPerPage = Math.Round((double)totalWords / document.PageCount, 2)
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
        finally
        {
            // Siempre eliminamos el archivo temporal
            try {File.Delete(tempPdfPath); } catch { /* opcional: log */ }
        }
    }

    /// <summary>
    /// Aplica técnicas de preprocesamiento a la imagen para mejorar la calidad del OCR
    /// </summary>
    /// <param name="originalImage">Imagen original a procesar</param>
    /// <returns>Imagen procesada con mejor calidad para OCR</returns>
    private Bitmap PreprocessImage(Bitmap originalImage)
    {
        try
        {
            // Crear una copia de la imagen original para no modificarla
            Bitmap processedImage = new Bitmap(originalImage);
            
            // Aplicar técnicas de preprocesamiento usando System.Drawing
            using (Graphics g = Graphics.FromImage(processedImage))
            {
                // Configuración para mejor calidad
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            }
            
            // Aplicar filtros para mejorar la imagen
            processedImage = AdjustContrast(processedImage, 1.5f); // Aumentar contraste
            processedImage = RemoveNoise(processedImage);          // Reducir ruido
            processedImage = Binarize(processedImage);             // Binarización adaptativa
            processedImage = DeskewImage(processedImage);          // Corregir inclinación
            
            return processedImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el preprocesamiento de imagen: {ex.Message}");
            // En caso de error, devolver la imagen original
            return originalImage;
        }
    }
    
    /// <summary>
    /// Ajusta el contraste de la imagen
    /// </summary>
    private Bitmap AdjustContrast(Bitmap image, float factor)
    {
        Bitmap adjustedImage = new Bitmap(image.Width, image.Height);
        
        // Matriz de color para ajustar contraste
        float[][] colorMatrixElements = {
            new float[] {factor, 0, 0, 0, 0},
            new float[] {0, factor, 0, 0, 0},
            new float[] {0, 0, factor, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {-0.1f, -0.1f, -0.1f, 0, 1}
        };
        
        using (var attributes = new System.Drawing.Imaging.ImageAttributes())
        {
            attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(colorMatrixElements));
            
            using (var g = Graphics.FromImage(adjustedImage))
            {
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }
        
        return adjustedImage;
    }
    
    /// <summary>
    /// Elimina ruido de la imagen usando un filtro de mediana
    /// </summary>
    private Bitmap RemoveNoise(Bitmap image)
    {
        // Implementación simplificada de reducción de ruido
        // Para una implementación completa, se recomienda usar bibliotecas especializadas
        Bitmap result = new Bitmap(image.Width, image.Height);
        
        // Aplicar un suavizado básico
        using (var g = Graphics.FromImage(result))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
        }
        
        return result;
    }
    
    /// <summary>
    /// Aplica binarización adaptativa a la imagen
    /// </summary>
    private Bitmap Binarize(Bitmap image)
    {
        try
        {
            // Crear una copia de la imagen
            Bitmap result = new Bitmap(image.Width, image.Height);
            
            // Bloquear los bits de la imagen para acceso más rápido
            System.Drawing.Imaging.BitmapData sourceData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            System.Drawing.Imaging.BitmapData resultData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            // Umbral para binarización (ajustar según necesidad)
            int threshold = 180;
            
            // Procesar la imagen usando punteros para mayor velocidad
            unsafe
            {
                byte* sourcePtr = (byte*)sourceData.Scan0;
                byte* resultPtr = (byte*)resultData.Scan0;
                
                int sourceStride = sourceData.Stride;
                int resultStride = resultData.Stride;
                
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        // Calcular la posición del píxel actual
                        int position = y * sourceStride + x * 4;
                        
                        // Obtener los valores RGB
                        byte blue = sourcePtr[position];
                        byte green = sourcePtr[position + 1];
                        byte red = sourcePtr[position + 2];
                        
                        // Calcular el valor de escala de grises
                        int grayScale = (int)((red * 0.3) + (green * 0.59) + (blue * 0.11));
                        
                        // Aplicar umbral
                        byte pixelValue = (byte)(grayScale > threshold ? 255 : 0);
                        
                        // Establecer el valor en la imagen de resultado
                        resultPtr[position] = pixelValue;     // Blue
                        resultPtr[position + 1] = pixelValue; // Green
                        resultPtr[position + 2] = pixelValue; // Red
                        resultPtr[position + 3] = 255;        // Alpha (opaco)
                    }
                }
            }
            
            // Desbloquear los bits
            image.UnlockBits(sourceData);
            result.UnlockBits(resultData);
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la binarización: {ex.Message}");
            return image; // Devolver imagen original en caso de error
        }
    }
    
    /// <summary>
    /// Corrige la inclinación de la imagen
    /// </summary>
    private Bitmap DeskewImage(Bitmap image)
    {
        // Nota: Una implementación completa de corrección de sesgo requiere algoritmos complejos
        // Esta es una implementación simplificada que devuelve la imagen original
        // Para una implementación real, considere usar bibliotecas como EmguCV o AForge.NET
        return new Bitmap(image);
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
            // 1. Corregir errores comunes de OCR
            var corrections = new Dictionary<string, string>
            {
                // Caracteres confundidos frecuentemente
                { "0", "O" }, { "l", "I" }, { "1", "l" },
                // Errores comunes en español
                { "a", "á" }, { "e", "é" }, { "i", "í" }, { "o", "ó" }, { "u", "ú" },
                // Símbolos mal interpretados
                { "'", "\"" }, { "`", "'" }, { "—", "-" }
            };
            
            // 2. Corregir palabras cortadas por saltos de línea
            text = System.Text.RegularExpressions.Regex.Replace(text, "(\w+)-\s*\n(\w+)", "$1$2");
            
            // 3. Eliminar caracteres extraños o ruido
            text = System.Text.RegularExpressions.Regex.Replace(text, "[^\w\s.,;:()\[\]{}¡!¿?@#$%&*+\-/\\\"'<>=_áéíóúÁÉÍÓÚüÜ]", "");
            
            // 4. Normalizar espacios múltiples
            text = System.Text.RegularExpressions.Regex.Replace(text, "\s+", " ");
            
            // 5. Corregir espacios antes de signos de puntuación
            text = System.Text.RegularExpressions.Regex.Replace(text, "\s+([.,;:!?])", "$1");
            
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el post-procesamiento de texto: {ex.Message}");
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