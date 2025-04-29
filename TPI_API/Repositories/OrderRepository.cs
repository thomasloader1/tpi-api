using Microsoft.EntityFrameworkCore;
using TPI_API.Context;
using TPI_API.Interfaces;
using TPI_API.Models;

namespace TPI_API.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly TPIDbContext _context;

    public OrderRepository(TPIDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        // Devuelve todas las tareas como una lista
        return await _context.Tareas.ToListAsync();
    }

    public async Task<Order> GetByIdAsync(int id)
    {
        // Busca una tarea específica por su ID
        return await _context.Tareas.FindAsync(id);
    }


public async Task AddAsync(Order tarea, string path)
    {
        // Crear una nueva instancia copiando las propiedades
        var nuevaTarea = new Order
        {
            Nombre = tarea.Nombre,
            Descripcion = tarea.Descripcion,
            Estado = tarea.Estado,
            FechaCreacion = DateTime.Now,
            FechaLimite = tarea.FechaLimite,
            UsuarioId = tarea.UsuarioId,
            FilePath = path // Seteás solo el path acá
        };

        await _context.Tareas.AddAsync(nuevaTarea);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(Order tarea, string path)
    {
        // Buscar la tarea original en la base de datos
        var tareaExistente = await _context.Tareas.FindAsync(tarea.Id);

        if (tareaExistente == null)
            throw new Exception("Tarea no encontrada.");

        // Actualizá los campos que necesites
        tareaExistente.Nombre = tarea.Nombre;
        tareaExistente.Descripcion = tarea.Descripcion;
        tareaExistente.Estado = tarea.Estado;
        tareaExistente.FechaCreacion = tarea.FechaCreacion;
        tareaExistente.FechaLimite = tarea.FechaLimite;
        tareaExistente.UsuarioId = tarea.UsuarioId;

        // Asigná el nuevo FilePath
        typeof(Order).GetProperty("FilePath")!
                     .SetValue(tareaExistente, path);

        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        // Busca y elimina una tarea
        var tarea = await GetByIdAsync(id);
        if (tarea != null)
        {
            _context.Tareas.Remove(tarea);
            await _context.SaveChangesAsync();
        }
    }


}
