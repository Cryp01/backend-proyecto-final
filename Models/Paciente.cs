using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace programacion_proyecto_backend.Models;

public class Paciente
{
  [Key]
  public Guid Id { get; set; }

  [Required]
  public string Nombre { get; set; } = string.Empty;

  [Column(TypeName = "numeric(10)")]
  public decimal? Telefono { get; set; }

  public string? Direccion { get; set; }

  [Column(TypeName = "date")]
  public DateTime? FechaNacimiento { get; set; }

  // Relación con Citas
  public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}

