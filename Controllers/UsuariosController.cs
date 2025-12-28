using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Models;
using programacion_proyecto_backend.DTOs;
using BCrypt.Net;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ILogger<UsuariosController> _logger;

  public UsuariosController(ApplicationDbContext context, ILogger<UsuariosController> logger)
  {
    _context = context;
    _logger = logger;
  }

    // GET: api/Usuarios
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
  {
    try
    {
      var usuarios = await _context.Usuarios
          .Include(u => u.Rol)
          .Select(u => new UsuarioDto
          {
            Id = u.Id,
            Nombre = u.Nombre,
            RolId = u.RolId,
            RolNombre = u.Rol.Nombre
          })
          .OrderBy(u => u.Nombre)
          .ToListAsync();

      return Ok(usuarios);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al obtener los usuarios");
      return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
    }
  }

    // GET: api/Usuarios/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
  {
    try
    {
      var usuario = await _context.Usuarios
          .Include(u => u.Rol)
          .Where(u => u.Id == id)
          .Select(u => new UsuarioDto
          {
            Id = u.Id,
            Nombre = u.Nombre,
            RolId = u.RolId,
            RolNombre = u.Rol.Nombre
          })
          .FirstOrDefaultAsync();

      if (usuario == null)
      {
        return NotFound(new { message = $"No se encontró el usuario con ID: {id}" });
      }

      return Ok(usuario);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al obtener el usuario {UsuarioId}", id);
      return StatusCode(500, new { message = "Error al obtener el usuario", error = ex.Message });
    }
  }

    // POST: api/Usuarios
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> CreateUsuario(CrearUsuarioDto crearUsuarioDto)
  {
    try
    {
      // Verificar que el nombre de usuario no exista
      var nombreExiste = await _context.Usuarios.AnyAsync(u => u.Nombre == crearUsuarioDto.Nombre);
      if (nombreExiste)
      {
        return BadRequest(new { message = "Ya existe un usuario con ese nombre" });
      }

      // Verificar que el rol existe
      var rolExiste = await _context.Roles.AnyAsync(r => r.Id == crearUsuarioDto.RolId);
      if (!rolExiste)
      {
        return BadRequest(new { message = "El rol especificado no existe" });
      }

      // Encriptar la contraseña
      var claveEncriptada = BCrypt.Net.BCrypt.HashPassword(crearUsuarioDto.Clave);

      var usuario = new Usuario
      {
        Id = Guid.NewGuid(),
        Nombre = crearUsuarioDto.Nombre,
        Clave = claveEncriptada,
        RolId = crearUsuarioDto.RolId
      };

      _context.Usuarios.Add(usuario);
      await _context.SaveChangesAsync();

      var usuarioDto = await _context.Usuarios
          .Include(u => u.Rol)
          .Where(u => u.Id == usuario.Id)
          .Select(u => new UsuarioDto
          {
            Id = u.Id,
            Nombre = u.Nombre,
            RolId = u.RolId,
            RolNombre = u.Rol.Nombre
          })
          .FirstOrDefaultAsync();

      _logger.LogInformation("Usuario creado exitosamente: {UsuarioId}", usuario.Id);
      return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuarioDto);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al crear el usuario");
      return StatusCode(500, new { message = "Error al crear el usuario", error = ex.Message });
    }
  }

    // PUT: api/Usuarios/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUsuario(Guid id, ActualizarUsuarioDto actualizarUsuarioDto)
  {
    try
    {
      var usuario = await _context.Usuarios.FindAsync(id);
      if (usuario == null)
      {
        return NotFound(new { message = $"No se encontró el usuario con ID: {id}" });
      }

      // Verificar que el nombre de usuario no exista en otro usuario
      var nombreExiste = await _context.Usuarios
          .AnyAsync(u => u.Nombre == actualizarUsuarioDto.Nombre && u.Id != id);
      if (nombreExiste)
      {
        return BadRequest(new { message = "Ya existe otro usuario con ese nombre" });
      }

      // Verificar que el rol existe
      var rolExiste = await _context.Roles.AnyAsync(r => r.Id == actualizarUsuarioDto.RolId);
      if (!rolExiste)
      {
        return BadRequest(new { message = "El rol especificado no existe" });
      }

      usuario.Nombre = actualizarUsuarioDto.Nombre;
      usuario.RolId = actualizarUsuarioDto.RolId;

      // Solo actualizar la contraseña si se proporciona una nueva
      if (!string.IsNullOrEmpty(actualizarUsuarioDto.Clave))
      {
        usuario.Clave = BCrypt.Net.BCrypt.HashPassword(actualizarUsuarioDto.Clave);
      }

      _context.Entry(usuario).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      _logger.LogInformation("Usuario actualizado exitosamente: {UsuarioId}", id);
      return NoContent();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al actualizar el usuario {UsuarioId}", id);
      return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
    }
  }

    // DELETE: api/Usuarios/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUsuario(Guid id)
  {
    try
    {
      var usuario = await _context.Usuarios.FindAsync(id);
      if (usuario == null)
      {
        return NotFound(new { message = $"No se encontró el usuario con ID: {id}" });
      }

      _context.Usuarios.Remove(usuario);
      await _context.SaveChangesAsync();

      _logger.LogInformation("Usuario eliminado exitosamente: {UsuarioId}", id);
      return NoContent();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al eliminar el usuario {UsuarioId}", id);
      return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
    }
  }
}

