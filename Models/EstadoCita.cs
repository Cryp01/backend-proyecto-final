using System.ComponentModel.DataAnnotations;

namespace programacion_proyecto_backend.Models;

public class EstadoCita
{
  [Key]
  public Guid Id { get; set; }

  [Required]
  public string Nombre { get; set; } = string.Empty;

  // Relación con Citas
  public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}

