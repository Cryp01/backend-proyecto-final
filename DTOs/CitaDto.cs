namespace programacion_proyecto_backend.DTOs;

public class CitaDto
{
  public Guid Id { get; set; }
  public Guid PacienteId { get; set; }
  public string NombrePaciente { get; set; } = string.Empty;
  public DateTime Fecha { get; set; }
  public Guid EstadoId { get; set; }
  public string NombreEstado { get; set; } = string.Empty;
  public string? NotaMedica { get; set; }
}

public class CrearCitaDto
{
  public Guid PacienteId { get; set; }
  public DateTime Fecha { get; set; }
  public Guid? EstadoId { get; set; } // Opcional, por defecto será "Pendiente"
  public string? NotaMedica { get; set; }
}

public class ActualizarCitaDto
{
  public Guid PacienteId { get; set; }
  public DateTime Fecha { get; set; }
  public Guid EstadoId { get; set; }
  public string? NotaMedica { get; set; }
}

