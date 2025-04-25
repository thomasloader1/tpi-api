using System.ComponentModel.DataAnnotations;

namespace TPI_API.Models;

public class Tarea
{
    [Key]
    public int Id { get; set; } 
    public string Nombre { get; set; }
    public string Descripcion { get; set; } 
    public string Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLimite { get; set; }
    public int UsuarioId { get; set; } 

}
