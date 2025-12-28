namespace programacion_proyecto_backend.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
}

public class CrearUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public Guid RolId { get; set; }
}

public class ActualizarUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Clave { get; set; }
    public Guid RolId { get; set; }
}

public class LoginDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public UsuarioDto Usuario { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

