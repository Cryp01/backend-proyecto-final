using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace programacion_proyecto_backend.Models;

public class Cita
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PacienteId { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public Guid EstadoId { get; set; }

    public string? NotaMedica { get; set; }

    // Relaciones
    [ForeignKey("PacienteId")]
    public Paciente Paciente { get; set; } = null!;

    [ForeignKey("EstadoId")]
    public EstadoCita Estado { get; set; } = null!;
}

