using Microsoft.EntityFrameworkCore;
using TPI_API.Context;
using TPI_API.Interfaces;
using TPI_API.Models;

namespace TPI_API.Repositories;

public class TareaRepository : ITareaRepository
{
    private readonly TPIDbContext _context;

    public TareaRepository(TPIDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tarea>> GetAllAsync()
    {
        // Devuelve todas las tareas como una lista
        return await _context.Tareas.ToListAsync();
    }

    public async Task<Tarea> GetByIdAsync(int id)
    {
        // Busca una tarea específica por su ID
        return await _context.Tareas.FindAsync(id);
    }

    public async Task AddAsync(Tarea tarea)
    {
        // Agrega una nueva tarea y guarda los cambios
        await _context.Tareas.AddAsync(tarea);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tarea tarea)
    {
        // Actualiza una tarea existente
        _context.Tareas.Update(tarea);
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
