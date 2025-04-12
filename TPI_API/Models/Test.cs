using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace TPI_API.Models;

public class Test
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Image { get; set; }
    public int AuthorId { get; set; }
}
