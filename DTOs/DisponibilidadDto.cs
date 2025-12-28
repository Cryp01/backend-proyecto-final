namespace programacion_proyecto_backend.DTOs;

public class DisponibilidadDiaDto
{
  public DateTime Fecha { get; set; }
  public List<HorarioDisponibleDto> Horarios { get; set; } = new();
}

public class HorarioDisponibleDto
{
  public TimeSpan Hora { get; set; }
  public string HoraFormateada { get; set; } = string.Empty;
  public bool Disponible { get; set; }
  public Guid? CitaId { get; set; }
}

public class DisponibilidadMesDto
{
  public int Anio { get; set; }
  public int Mes { get; set; }
  public List<ResumenDiaDto> Dias { get; set; } = new();
}

public class ResumenDiaDto
{
  public DateTime Fecha { get; set; }
  public int TotalEspacios { get; set; }
  public int EspaciosOcupados { get; set; }
  public int EspaciosDisponibles { get; set; }
  public bool TieneDisponibilidad { get; set; }
}

