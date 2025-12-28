using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Models;
using programacion_proyecto_backend.DTOs;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ILogger<PacientesController> _logger;

  public PacientesController(ApplicationDbContext context, ILogger<PacientesController> logger)
  {
    _context = context;
    _logger = logger;
  }

  // GET: api/Pacientes
  [HttpGet]
  [Authorize]
  public async Task<ActionResult<IEnumerable<PacienteDto>>> GetPacientes()
  {
    try
    {
      var pacientes = await _context.Pacientes
          .Select(p => new PacienteDto
          {
            Id = p.Id,
            Nombre = p.Nombre,
            Telefono = p.Telefono,
            Direccion = p.Direccion,
            FechaNacimiento = p.FechaNacimiento,
            TotalCitas = p.Citas.Count
          })
          .OrderBy(p => p.Nombre)
          .ToListAsync();

      return Ok(pacientes);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al obtener los pacientes");
      return StatusCode(500, new { message = "Error al obtener los pacientes", error = ex.Message });
    }
  }

  // GET: api/Pacientes/{id}
  [HttpGet("{id}")]
  [AllowAnonymous]
  public async Task<ActionResult<PacienteDto>> GetPaciente(Guid id)
  {
    try
    {
      var paciente = await _context.Pacientes
          .Where(p => p.Id == id)
          .Select(p => new PacienteDto
          {
            Id = p.Id,
            Nombre = p.Nombre,
            Telefono = p.Telefono,
            Direccion = p.Direccion,
            FechaNacimiento = p.FechaNacimiento,
            TotalCitas = p.Citas.Count
          })
          .FirstOrDefaultAsync();

      if (paciente == null)
      {
        return NotFound(new { message = $"No se encontró el paciente con ID: {id}" });
      }

      return Ok(paciente);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al obtener el paciente {PacienteId}", id);
      return StatusCode(500, new { message = "Error al obtener el paciente", error = ex.Message });
    }
  }

  // GET: api/Pacientes/Telefono/{telefono}
  [HttpGet("Telefono/{telefono}")]
  [AllowAnonymous]
  public async Task<ActionResult<PacienteDto>> GetPacientePorTelefono(decimal telefono)
  {
    try
    {
      var paciente = await _context.Pacientes
          .Where(p => p.Telefono == telefono)
          .Select(p => new PacienteDto
          {
            Id = p.Id,
            Nombre = p.Nombre,
            Telefono = p.Telefono,
            Direccion = p.Direccion,
            FechaNacimiento = p.FechaNacimiento,
            TotalCitas = p.Citas.Count
          })
          .FirstOrDefaultAsync();

      if (paciente == null)
      {
        return NotFound(new { message = $"No se encontró el paciente con teléfono: {telefono}" });
      }

      return Ok(paciente);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al obtener el paciente con teléfono {Telefono}", telefono);
      return StatusCode(500, new { message = "Error al obtener el paciente", error = ex.Message });
    }
  }

  // POST: api/Pacientes
  [HttpPost]
  [AllowAnonymous]
  public async Task<ActionResult<PacienteDto>> CreatePaciente(CrearPacienteDto crearPacienteDto)
  {
    try
    {
      var paciente = new Paciente
      {
        Id = Guid.NewGuid(),
        Nombre = crearPacienteDto.Nombre,
        Telefono = crearPacienteDto.Telefono,
        Direccion = crearPacienteDto.Direccion,
        FechaNacimiento = crearPacienteDto.FechaNacimiento
      };

      _context.Pacientes.Add(paciente);
      await _context.SaveChangesAsync();

      var pacienteDto = new PacienteDto
      {
        Id = paciente.Id,
        Nombre = paciente.Nombre,
        Telefono = paciente.Telefono,
        Direccion = paciente.Direccion,
        FechaNacimiento = paciente.FechaNacimiento,
        TotalCitas = 0
      };

      _logger.LogInformation("Paciente creado exitosamente: {PacienteId}", paciente.Id);
      return CreatedAtAction(nameof(GetPaciente), new { id = paciente.Id }, pacienteDto);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al crear el paciente");
      return StatusCode(500, new { message = "Error al crear el paciente", error = ex.Message });
    }
  }

  // PUT: api/Pacientes/{id}
  [HttpPut("{id}")]
  [Authorize]
  public async Task<IActionResult> UpdatePaciente(Guid id, ActualizarPacienteDto actualizarPacienteDto)
  {
    try
    {
      var paciente = await _context.Pacientes.FindAsync(id);
      if (paciente == null)
      {
        return NotFound(new { message = $"No se encontró el paciente con ID: {id}" });
      }

      paciente.Nombre = actualizarPacienteDto.Nombre;
      paciente.Telefono = actualizarPacienteDto.Telefono;
      paciente.Direccion = actualizarPacienteDto.Direccion;
      paciente.FechaNacimiento = actualizarPacienteDto.FechaNacimiento;

      _context.Entry(paciente).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      _logger.LogInformation("Paciente actualizado exitosamente: {PacienteId}", id);
      return NoContent();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al actualizar el paciente {PacienteId}", id);
      return StatusCode(500, new { message = "Error al actualizar el paciente", error = ex.Message });
    }
  }

  // DELETE: api/Pacientes/{id}
  [HttpDelete("{id}")]
  [Authorize]
  public async Task<IActionResult> DeletePaciente(Guid id)
  {
    try
    {
      var paciente = await _context.Pacientes
          .Include(p => p.Citas)
          .FirstOrDefaultAsync(p => p.Id == id);

      if (paciente == null)
      {
        return NotFound(new { message = $"No se encontró el paciente con ID: {id}" });
      }

      // Verificar si tiene citas asociadas
      if (paciente.Citas.Any())
      {
        return BadRequest(new
        {
          message = "No se puede eliminar el paciente porque tiene citas asociadas",
          totalCitas = paciente.Citas.Count
        });
      }

      _context.Pacientes.Remove(paciente);
      await _context.SaveChangesAsync();

      _logger.LogInformation("Paciente eliminado exitosamente: {PacienteId}", id);
      return NoContent();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al eliminar el paciente {PacienteId}", id);
      return StatusCode(500, new { message = "Error al eliminar el paciente", error = ex.Message });
    }
  }
}

