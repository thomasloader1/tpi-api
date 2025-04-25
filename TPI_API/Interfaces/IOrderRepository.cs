using TPI_API.Models;

namespace TPI_API.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> GetByIdAsync(int id);
    Task AddAsync(Order tarea);
    Task UpdateAsync(Order tarea);
    Task DeleteAsync(int id); 
}
