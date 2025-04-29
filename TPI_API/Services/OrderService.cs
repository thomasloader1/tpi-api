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

    public async Task CrearAsync(Order tarea, string path)
    {
        // Agregar una nueva tarea al repositorio
        await _tareaRepository.AddAsync(tarea, path);
    }

    public async Task ActualizarAsync(Order tarea, string path)
    {
        // Actualizar una tarea existente
        await _tareaRepository.UpdateAsync(tarea, path);
    }

    public async Task EliminarAsync(int id)
    {
        // Eliminar una tarea por su ID
        await _tareaRepository.DeleteAsync(id);
    }

    public Task<string> GuardarArchivo(IFormFile file, string carpetaDestino)
    {
        // Guardar un archivo en una carpeta específica
        string filePath = Path.Combine(carpetaDestino, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        // Guardar la ruta del archivo en la base de datos
        var tarea = new Order
        {
            FilePath = filePath
        };

        return Task.FromResult(filePath);
    }

    public Task<string> ObtenerArchivoDesdeBaseDeDatos(string id)
    {
        //obtener el archivo desde el tarea repository
        var tarea = _tareaRepository.GetByIdAsync(int.Parse(id));
        if (tarea != null)
        {
            return Task.FromResult(tarea.Result.FilePath);
        }
        return Task.FromResult(string.Empty);

    }
}
