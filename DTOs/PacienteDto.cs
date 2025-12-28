namespace programacion_proyecto_backend.DTOs;

public class PacienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int TotalCitas { get; set; }
}

public class CrearPacienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}

public class ActualizarPacienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}

