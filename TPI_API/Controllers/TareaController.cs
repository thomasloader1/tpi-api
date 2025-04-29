using System.IO;
using Microsoft.AspNetCore.Mvc;
using TPI_API.Interfaces;
using TPI_API.Models;

[ApiController]
[Route("api/[controller]")]
public class TareaController : ControllerBase
{
    private readonly IOrderService _tareaService;

    public TareaController(IOrderService tareaService)
    {
        _tareaService = tareaService;
    }

    // Obtener todas las tareas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return Ok(tareas); // Devuelve un listado de tareas
    }

    // Obtener una tarea específica por su ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);
        if (tarea == null)
        {
            return NotFound($"Tarea con ID {id} no encontrada.");
        }
        return Ok(tarea);
    }

    // Update the Create method to correctly handle the return type of UploadFile
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] TPI_API.Models.Order tarea, IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string path = await ObtenerResultadoOk(UploadFile(file));

        await _tareaService.CrearAsync(tarea,path);

        return CreatedAtAction(nameof(GetById), new { id = tarea.Id }, tarea);
    }

    // Actualizar una tarea existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] TPI_API.Models.Order tarea, IFormFile? file)
    {
        if (id != tarea.Id)
        {
            return BadRequest("El ID proporcionado no coincide con el ID de la tarea.");
        }

        var path = (await _tareaService.ObtenerPorIdAsync(id)).FilePath;

        if (file != null && file.Length != 0)
        {
            path = await ObtenerResultadoOk(UploadFile(file));
        }


        await _tareaService.ActualizarAsync(tarea, path);
        return NoContent(); // Indica que la operación fue exitosa pero no devuelve contenido
    }

    // Eliminar una tarea por su ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tareaService.EliminarAsync(id);
        return NoContent();
    }

    // Guardar un archivo en la carpeta Uploads, recibiendolo con un post
    private async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha proporcionado un archivo.");
        }

        string carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        string resultado = await _tareaService.GuardarArchivo(file, carpetaDestino);
        return Ok(resultado);
    }

    //Obtener el archivo id desde la carpeta Uploads, recibiendolo con un get.
    [HttpGet("download/{id}")]
    public IActionResult DownloadFile(int idtarea)
    {
        string carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        // obtener la tarea por id
        var tarea = _tareaService.ObtenerPorIdAsync(idtarea).Result;

        string filePath = tarea.FilePath;
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("El archivo no existe.");
        }
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var fileName = Path.GetFileName(filePath);
        return File(fileStream, "application/octet-stream", fileName);
    }

    private async Task<string?> ObtenerResultadoOk(Task<IActionResult> accion)
{
    var result = await accion;
    
    if (result is OkObjectResult okResult)
    {
        return okResult.Value?.ToString();
    }

    return null; // o lanzar una excepción si preferís
}




}
