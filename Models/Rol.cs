using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace programacion_proyecto_backend.Models;

public class Rol
{
  [Key]
  public Guid Id { get; set; }

  [Required]
  public string Nombre { get; set; } = string.Empty;

  // Relación con Usuarios
  public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

