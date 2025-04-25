using TPI_API.Models;

namespace TPI_API.Interfaces;

public interface ITareaService
{
    Task<IEnumerable<Tarea>> ObtenerTodasAsync();
    Task<Tarea> ObtenerPorIdAsync(int id);
    Task CrearAsync(Tarea tarea);
    Task ActualizarAsync(Tarea tarea);
    Task EliminarAsync(int id);
}
