using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Intenta conectarse a la base de datos
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                return Ok(new
                {
                    status = "OK",
                    message = "La API está funcionando correctamente",
                    database = "Conectado a PostgreSQL",
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "No se pudo conectar a la base de datos",
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la conexión a la base de datos");
            return StatusCode(500, new
            {
                status = "ERROR",
                message = "Error al verificar la conexión a la base de datos",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

