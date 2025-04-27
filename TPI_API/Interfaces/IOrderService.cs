using TPI_API.Models;

namespace TPI_API.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<Order>> ObtenerTodasAsync();
    Task<Order> ObtenerPorIdAsync(int id);
    Task CrearAsync(Order tarea);
    Task ActualizarAsync(Order tarea);
    Task EliminarAsync(int id);

    Task<string> GuardarArchivo(IFormFile file, string carpetaDestino);
}
