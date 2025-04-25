using TPI_API.Interfaces;
using TPI_API.Models;

namespace TPI_API.Services;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _tareaRepository;

    public OrderService(IOrderRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IEnumerable<Order>> ObtenerTodasAsync()
    {
        // Obtener todas las tareas del repositorio
        return await _tareaRepository.GetAllAsync();
    }

    public async Task<Order> ObtenerPorIdAsync(int id)
    {
        // Obtener una tarea específica por su ID
        return await _tareaRepository.GetByIdAsync(id);
    }

    public async Task CrearAsync(Order tarea)
    {
        // Agregar una nueva tarea al repositorio
        await _tareaRepository.AddAsync(tarea);
    }

    public async Task ActualizarAsync(Order tarea)
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
