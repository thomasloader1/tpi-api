using Microsoft.AspNetCore.Mvc;
using TPI_API.Interfaces;
using TPI_API.Models;

[ApiController]
[Route("api/[controller]")]
public class TareaController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareaController(ITareaService tareaService)
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

    // Crear una nueva tarea
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Tarea tarea)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _tareaService.CrearAsync(tarea);
        return CreatedAtAction(nameof(GetById), new { id = tarea.Id }, tarea);
    }

    // Actualizar una tarea existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Tarea tarea)
    {
        if (id != tarea.Id)
        {
            return BadRequest("El ID proporcionado no coincide con el ID de la tarea.");
        }
        await _tareaService.ActualizarAsync(tarea);
        return NoContent(); // Indica que la operación fue exitosa pero no devuelve contenido
    }

    // Eliminar una tarea por su ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tareaService.EliminarAsync(id);
        return NoContent();
    }
}
