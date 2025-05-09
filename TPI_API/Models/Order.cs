﻿using System.ComponentModel.DataAnnotations;

namespace TPI_API.Models;

public class Order
{
    [Key]
    public int Id { get; set; } 
    public string Nombre { get; set; }
    public string Descripcion { get; set; } 
    public string Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLimite { get; set; }
    public int? UsuarioId { get; set; }
    public string? FilePath { get; internal set; }
}
