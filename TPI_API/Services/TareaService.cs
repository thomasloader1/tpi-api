using TPI_API.Interfaces;
using TPI_API.Models;

namespace TPI_API.Services;
public class TareaService : ITareaService
{
    private readonly ITareaRepository _tareaRepository;

    public TareaService(ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IEnumerable<Tarea>> ObtenerTodasAsync()
    {
        // Obtener todas las tareas del repositorio
        return await _tareaRepository.GetAllAsync();
    }

    public async Task<Tarea> ObtenerPorIdAsync(int id)
    {
        // Obtener una tarea específica por su ID
        return await _tareaRepository.GetByIdAsync(id);
    }

    public async Task CrearAsync(Tarea tarea)
    {
        // Agregar una nueva tarea al repositorio
        await _tareaRepository.AddAsync(tarea);
    }

    public async Task ActualizarAsync(Tarea tarea)
    {
        // Actualizar una tarea existente
        await _tareaRepository.UpdateAsync(tarea);
    }

    public async Task EliminarAsync(int id)
    {
        // Eliminar una tarea por su ID
        await _tareaRepository.DeleteAsync(id);
    }
}
