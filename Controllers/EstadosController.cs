using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.DTOs;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstadosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EstadosController> _logger;

    public EstadosController(ApplicationDbContext context, ILogger<EstadosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Estados
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EstadoDto>>> GetEstados()
    {
        try
        {
            var estados = await _context.EstadosCita
                .AsNoTracking()
                .Select(e => new EstadoDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre
                })
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return Ok(estados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los estados de cita");
            return StatusCode(500, new { message = "Error al obtener los estados", error = ex.Message });
        }
    }

    // GET: api/Estados/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<EstadoDto>> GetEstado(Guid id)
    {
        try
        {
            var estado = await _context.EstadosCita
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new EstadoDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre
                })
                .FirstOrDefaultAsync();

            if (estado == null)
            {
                return NotFound(new { message = $"No se encontró el estado con ID: {id}" });
            }

            return Ok(estado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el estado {EstadoId}", id);
            return StatusCode(500, new { message = "Error al obtener el estado", error = ex.Message });
        }
    }
}

