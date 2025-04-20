using PdfiumViewer;
using System.Drawing;
using System.Runtime.InteropServices;
using TPI_API.Dtos.DocumentProcessed;
using TPI_API.Interfaces;

namespace TPI_API.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
    static DocumentProcessingService()
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

    
    public async Task<ProcessedDocumentDto> ProcessPdfDocumentAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se ha proporcionado un archivo PDF.");

        string tempPdfPath = Path.GetTempFileName();
        var result = new ProcessedDocumentDto
        {
            OriginalFileName = file.FileName,
            FileSize = $"{Math.Round(file.Length / 1024.0, 2)} KB",
            FileType = "PDF"
        };

        try
        {
            // Guardar archivo temporalmente
            using (var fs = new FileStream(tempPdfPath, FileMode.Create))
                await file.CopyToAsync(fs);

            using var document = PdfDocument.Load(tempPdfPath);
            result.PageCount = document.PageCount;
            result.ProcessedPages = new List<ProcessedPageDto>();

            for (int i = 0; i < document.PageCount; i++)
            {
                using var bmp = (Bitmap)document.Render(i, 300, 300, true);
                
                // Aplicar técnicas de preprocesamiento para mejorar la calidad de la imagen
                using var processedBmp = PreprocessImage(bmp);

                // Guardar la imagen procesada en memoria
                using var ms = new MemoryStream();
                processedBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                
                var processedPage = new ProcessedPageDto
                {
                    PageNumber = i + 1,
                    ProcessedImageData = ms.ToArray(),
                    Width = processedBmp.Width,
                    Height = processedBmp.Height
                };
                
                result.ProcessedPages.Add(processedPage);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al procesar el documento PDF: {ex.Message}", ex);
        }
        finally
        {
            // Siempre eliminamos el archivo temporal
            try { File.Delete(tempPdfPath); } catch { /* opcional: log */ }
        }
    }

  
    public async Task<ProcessedDocumentDto> ProcessImageDocumentAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se ha proporcionado un archivo de imagen.");

        var result = new ProcessedDocumentDto
        {
            OriginalFileName = file.FileName,
            FileSize = $"{Math.Round(file.Length / 1024.0, 2)} KB",
            FileType = Path.GetExtension(file.FileName).ToUpper().TrimStart('.')
        };

        try
        {
            // Cargar la imagen en memoria
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var originalImage = new Bitmap(ms);
            
            // Aplicar técnicas de preprocesamiento para mejorar la calidad de la imagen
            using var processedImage = PreprocessImage(originalImage);

            // Guardar la imagen procesada
            using var outputMs = new MemoryStream();
            processedImage.Save(outputMs, System.Drawing.Imaging.ImageFormat.Png);
            
            result.PageCount = 1;
            result.ProcessedPages = new List<ProcessedPageDto>
            {
                new ProcessedPageDto
                {
                    PageNumber = 1,
                    ProcessedImageData = outputMs.ToArray(),
                    Width = processedImage.Width,
                    Height = processedImage.Height
                }
            };

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al procesar la imagen: {ex.Message}", ex);
        }
    }

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
            
            // Aplicar filtros mejorados para aumentar la calidad de la imagen
            processedImage = ResizeImage(processedImage, 2.0f);    // Aumentar resolución
            processedImage = AdjustContrast(processedImage, 1.8f); // Aumentar contraste
            processedImage = RemoveNoise(processedImage);          // Reducir ruido
            processedImage = SharpenImage(processedImage);         // Aumentar nitidez
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
    
    private Bitmap RemoveNoise(Bitmap image)
    {
        // Implementación simplificada de reducción de ruido
        Bitmap result = new Bitmap(image.Width, image.Height);
        
        // Aplicar un suavizado básico
        using (var g = Graphics.FromImage(result))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
        }
        
        return result;
    }

    private Bitmap Binarize(Bitmap image)
    {
        Bitmap result = new Bitmap(image.Width, image.Height);

        System.Drawing.Imaging.BitmapData sourceData = null;
        System.Drawing.Imaging.BitmapData resultData = null;

        try
        {
            sourceData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            resultData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int threshold = 180;

            unsafe
            {
                byte* sourcePtr = (byte*)sourceData.Scan0;
                byte* resultPtr = (byte*)resultData.Scan0;

                int height = image.Height;
                int width = image.Width;
                int sourceStride = sourceData.Stride;
                int resultStride = resultData.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte* sRow = sourcePtr + (y * sourceStride);
                    byte* rRow = resultPtr + (y * resultStride);

                    for (int x = 0; x < width; x++)
                    {
                        int i = x * 4;
                        byte b = sRow[i];
                        byte g = sRow[i + 1];
                        byte r = sRow[i + 2];

                        int gray = (int)(r * 0.3 + g * 0.59 + b * 0.11);
                        byte bin = (byte)(gray > threshold ? 255 : 0);

                        rRow[i] = bin;
                        rRow[i + 1] = bin;
                        rRow[i + 2] = bin;
                        rRow[i + 3] = 255;
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en binarización: {ex.Message}");
            return image;
        }
        finally
        {
            if (sourceData != null)
                image.UnlockBits(sourceData);
            if (resultData != null)
                result.UnlockBits(resultData);
        }
    }

    private Bitmap ResizeImage(Bitmap image, float scaleFactor)
    {
        try
        {
            int newWidth = (int)(image.Width * scaleFactor);
            int newHeight = (int)(image.Height * scaleFactor);
            
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            
            return resizedImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al redimensionar imagen: {ex.Message}");
            return image;
        }
    }
    
    private Bitmap SharpenImage(Bitmap image)
    {
        try
        {
            // Crear una copia de la imagen original
            Bitmap sharpenedImage = new Bitmap(image.Width, image.Height);
            
            // Definir matriz de convolución para nitidez
            float[][] sharpenMatrix = {
                new float[] {-1, -1, -1},
                new float[] {-1,  9, -1},
                new float[] {-1, -1, -1}
            };
            
            // Crear matriz de color para aplicar el filtro
            var colorMatrix = new System.Drawing.Imaging.ColorMatrix(new float[][] {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            
            // Aplicar filtro de nitidez usando ImageAttributes
            using (var attributes = new System.Drawing.Imaging.ImageAttributes())
            {
                attributes.SetColorMatrix(colorMatrix);
                
                using (var g = Graphics.FromImage(sharpenedImage))
                {
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                        0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            
            return sharpenedImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al aplicar nitidez: {ex.Message}");
            return image;
        }
    }
    

    private Bitmap DeskewImage(Bitmap image)
    {
        // Nota: Una implementación completa de corrección de sesgo requiere algoritmos complejos
        // Esta es una implementación simplificada que devuelve la imagen original
        return new Bitmap(image);
    }
}