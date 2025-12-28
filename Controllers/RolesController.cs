using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Models;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RolesController> _logger;

    public RolesController(ApplicationDbContext context, ILogger<RolesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Roles
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
    {
        try
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los roles");
            return StatusCode(500, new { message = "Error al obtener los roles", error = ex.Message });
        }
    }

    // GET: api/Roles/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Rol>> GetRol(Guid id)
    {
        try
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
            {
                return NotFound(new { message = $"No se encontró el rol con ID: {id}" });
            }

            return Ok(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el rol {RolId}", id);
            return StatusCode(500, new { message = "Error al obtener el rol", error = ex.Message });
        }
    }
}

