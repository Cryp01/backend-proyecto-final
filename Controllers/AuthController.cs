using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.DTOs;
using programacion_proyecto_backend.Models;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    // POST: api/Auth/Login
    [HttpPost("Login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        try
        {
            // Buscar usuario por nombre
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Nombre == loginDto.Nombre);

            if (usuario == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            // Verificar contraseña
            var claveValida = BCrypt.Net.BCrypt.Verify(loginDto.Clave, usuario.Clave);
            if (!claveValida)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            // Generar token JWT
            var token = GenerateJwtToken(usuario);

            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                RolId = usuario.RolId,
                RolNombre = usuario.Rol.Nombre
            };

            var response = new LoginResponseDto
            {
                Usuario = usuarioDto,
                Token = token
            };

            _logger.LogInformation("Usuario {Nombre} inició sesión exitosamente", usuario.Nombre);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar login");
            return StatusCode(500, new { message = "Error al realizar login", error = ex.Message });
        }
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var jwtKey = _configuration["JWT_KEY"] 
            ?? Environment.GetEnvironmentVariable("JWT_KEY")
            ?? _configuration["Jwt:Key"]
            ?? "TuClaveSecretaSuperSeguraQueDebeTenerAlMenos32Caracteres2024!";
            
        var jwtIssuer = _configuration["JWT_ISSUER"]
            ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? _configuration["Jwt:Issuer"]
            ?? "ProgramacionProyectoBackend";
            
        var jwtAudience = _configuration["JWT_AUDIENCE"]
            ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? _configuration["Jwt:Audience"]
            ?? "ProgramacionProyectoBackend";
            
        var jwtExpiryMinutes = int.Parse(
            _configuration["JWT_EXPIRY_MINUTES"]
            ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
            ?? _configuration["Jwt:ExpiryMinutes"]
            ?? "1440"); // 24 horas por defecto

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
            new Claim("RolId", usuario.RolId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

