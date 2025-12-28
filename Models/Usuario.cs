using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace programacion_proyecto_backend.Models;

public class Usuario
{
  [Key]
  public Guid Id { get; set; }

  [Required]
  public string Nombre { get; set; } = string.Empty;

  [Required]
  public string Clave { get; set; } = string.Empty;

  [Required]
  public Guid RolId { get; set; }

  // Relación con Rol
  [ForeignKey("RolId")]
  public Rol Rol { get; set; } = null!;
}

