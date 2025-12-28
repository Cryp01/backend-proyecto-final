using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Models;
using programacion_proyecto_backend.DTOs;

namespace programacion_proyecto_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CitasController> _logger;

    public CitasController(ApplicationDbContext context, ILogger<CitasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Citas
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CitaDto>>> GetCitas()
    {
        try
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Estado)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new CitaDto
                {
                    Id = c.Id,
                    PacienteId = c.PacienteId,
                    NombrePaciente = c.Paciente.Nombre,
                    Fecha = c.Fecha,
                    EstadoId = c.EstadoId,
                    NombreEstado = c.Estado.Nombre,
                    NotaMedica = c.NotaMedica
                })
                .ToListAsync();

            return Ok(citas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las citas");
            return StatusCode(500, new { message = "Error al obtener las citas", error = ex.Message });
        }
    }

    // GET: api/Citas/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CitaDto>> GetCita(Guid id)
    {
        try
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Estado)
                .Where(c => c.Id == id)
                .Select(c => new CitaDto
                {
                    Id = c.Id,
                    PacienteId = c.PacienteId,
                    NombrePaciente = c.Paciente.Nombre,
                    Fecha = c.Fecha,
                    EstadoId = c.EstadoId,
                    NombreEstado = c.Estado.Nombre,
                    NotaMedica = c.NotaMedica
                })
                .FirstOrDefaultAsync();

            if (cita == null)
            {
                return NotFound(new { message = $"No se encontró la cita con ID: {id}" });
            }

            return Ok(cita);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la cita {CitaId}", id);
            return StatusCode(500, new { message = "Error al obtener la cita", error = ex.Message });
        }
    }

    // GET: api/Citas/Paciente/{pacienteId}
    [HttpGet("Paciente/{pacienteId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CitaDto>>> GetCitasPorPaciente(Guid pacienteId)
    {
        try
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Estado)
                .Where(c => c.PacienteId == pacienteId)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new CitaDto
                {
                    Id = c.Id,
                    PacienteId = c.PacienteId,
                    NombrePaciente = c.Paciente.Nombre,
                    Fecha = c.Fecha,
                    EstadoId = c.EstadoId,
                    NombreEstado = c.Estado.Nombre,
                    NotaMedica = c.NotaMedica
                })
                .ToListAsync();

            return Ok(citas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las citas del paciente {PacienteId}", pacienteId);
            return StatusCode(500, new { message = "Error al obtener las citas del paciente", error = ex.Message });
        }
    }

    // POST: api/Citas
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CitaDto>> CreateCita(CrearCitaDto crearCitaDto)
    {
        try
        {
            // Verificar que el paciente existe
            var pacienteExiste = await _context.Pacientes.AnyAsync(p => p.Id == crearCitaDto.PacienteId);
            if (!pacienteExiste)
            {
                return BadRequest(new { message = "El paciente especificado no existe" });
            }

            // Si no se proporciona estado, buscar el estado "Pendiente" por defecto
            Guid estadoId;
            if (crearCitaDto.EstadoId.HasValue)
            {
                estadoId = crearCitaDto.EstadoId.Value;
            }
            else
            {
                // Buscar el ID del estado "Pendiente"
                var estadoPendiente = await _context.EstadosCita
                    .Where(e => e.Nombre == "Pendiente")
                    .FirstOrDefaultAsync();
                if (estadoPendiente == null)
                {
                    return BadRequest(new { message = "No se encontró el estado 'Pendiente' en la base de datos" });
                }
                Console.WriteLine("Estado Pendiente: " + estadoPendiente.Id);
                estadoId = estadoPendiente.Id;
            }

            // Verificar que el estado existe
            var estadoExiste = await _context.EstadosCita.AnyAsync(e => e.Id == estadoId);
            Console.WriteLine("Estado ID: " + estadoId);
            Console.WriteLine("Estado Existe: " + estadoExiste);
            if (!estadoExiste)
            {
                return BadRequest(new { message = "El estado especificado no existe" });
            }

            // Obtener el estado para verificar su nombre
            var estado = await _context.EstadosCita.FindAsync(estadoId);
            if (estado == null)
            {
                return BadRequest(new { message = "El estado especificado no existe" });
            }

            // Validar que el paciente no tenga más de una cita pendiente
            if (estado.Nombre == "Pendiente")
            {
                var tieneCitaPendiente = await _context.Citas
                    .Include(c => c.Estado)
                    .AnyAsync(c => c.PacienteId == crearCitaDto.PacienteId &&
                                  c.Estado.Nombre == "Pendiente");

                if (tieneCitaPendiente)
                {
                    return BadRequest(new
                    {
                        message = "El paciente ya tiene una cita pendiente. Solo puede tener una cita pendiente a la vez."
                    });
                }
            }

            var cita = new Cita
            {
                Id = Guid.NewGuid(),
                PacienteId = crearCitaDto.PacienteId,
                Fecha = crearCitaDto.Fecha,
                EstadoId = estadoId,
                NotaMedica = crearCitaDto.NotaMedica
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Obtener la cita creada con sus relaciones
            var citaCreada = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Estado)
                .Where(c => c.Id == cita.Id)
                .Select(c => new CitaDto
                {
                    Id = c.Id,
                    PacienteId = c.PacienteId,
                    NombrePaciente = c.Paciente.Nombre,
                    Fecha = c.Fecha,
                    EstadoId = c.EstadoId,
                    NombreEstado = c.Estado.Nombre,
                    NotaMedica = c.NotaMedica
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Cita creada exitosamente: {CitaId}", cita.Id);
            return CreatedAtAction(nameof(GetCita), new { id = cita.Id }, citaCreada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la cita");
            return StatusCode(500, new { message = "Error al crear la cita", error = ex.Message });
        }
    }

    // PUT: api/Citas/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCita(Guid id, ActualizarCitaDto actualizarCitaDto)
    {
        try
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound(new { message = $"No se encontró la cita con ID: {id}" });
            }

            // Verificar que el paciente existe
            var pacienteExiste = await _context.Pacientes.AnyAsync(p => p.Id == actualizarCitaDto.PacienteId);
            if (!pacienteExiste)
            {
                return BadRequest(new { message = "El paciente especificado no existe" });
            }

            // Verificar que el estado existe
            var estado = await _context.EstadosCita.FindAsync(actualizarCitaDto.EstadoId);
            if (estado == null)
            {
                return BadRequest(new { message = "El estado especificado no existe" });
            }

            // Validar que el paciente no tenga más de una cita pendiente (excluyendo la cita actual)
            if (estado.Nombre == "Pendiente")
            {
                var tieneOtraCitaPendiente = await _context.Citas
                    .Include(c => c.Estado)
                    .AnyAsync(c => c.PacienteId == actualizarCitaDto.PacienteId &&
                                  c.Estado.Nombre == "Pendiente" &&
                                  c.Id != id);

                if (tieneOtraCitaPendiente)
                {
                    return BadRequest(new
                    {
                        message = "El paciente ya tiene una cita pendiente. Solo puede tener una cita pendiente a la vez."
                    });
                }
            }

            cita.PacienteId = actualizarCitaDto.PacienteId;
            cita.Fecha = actualizarCitaDto.Fecha;
            cita.EstadoId = actualizarCitaDto.EstadoId;
            cita.NotaMedica = actualizarCitaDto.NotaMedica;

            _context.Entry(cita).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cita actualizada exitosamente: {CitaId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la cita {CitaId}", id);
            return StatusCode(500, new { message = "Error al actualizar la cita", error = ex.Message });
        }
    }

    // DELETE: api/Citas/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCita(Guid id)
    {
        try
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound(new { message = $"No se encontró la cita con ID: {id}" });
            }

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cita eliminada exitosamente: {CitaId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la cita {CitaId}", id);
            return StatusCode(500, new { message = "Error al eliminar la cita", error = ex.Message });
        }
    }

    // GET: api/Citas/Disponibilidad/Dia?fecha=2024-01-15
    [HttpGet("Disponibilidad/Dia")]
    [AllowAnonymous]
    public async Task<ActionResult<DisponibilidadDiaDto>> GetDisponibilidadDia([FromQuery] DateTime fecha)
    {
        try
        {
            // Generar todos los horarios disponibles de 8:00 AM a 12:30 PM (cada 30 minutos)
            var horarios = new List<HorarioDisponibleDto>();
            var horaInicio = new TimeSpan(8, 0, 0);  // 8:00 AM
            var horaFin = new TimeSpan(12, 30, 0);   // 12:30 PM
            var intervalo = TimeSpan.FromMinutes(30);

            // Obtener todas las citas del día (comparar por año, mes y día)
            var citasDelDia = await _context.Citas
                .Where(c => c.Fecha.Year == fecha.Year &&
                           c.Fecha.Month == fecha.Month &&
                           c.Fecha.Day == fecha.Day)
                .ToListAsync();

            // Generar horarios y verificar disponibilidad
            for (var hora = horaInicio; hora <= horaFin; hora += intervalo)
            {
                var fechaHoraCompleta = fecha.Date.Add(hora);
                var citaEnHorario = citasDelDia.FirstOrDefault(c =>
                    c.Fecha.Hour == hora.Hours &&
                    c.Fecha.Minute == hora.Minutes);

                horarios.Add(new HorarioDisponibleDto
                {
                    Hora = hora,
                    HoraFormateada = hora.ToString(@"hh\:mm"),
                    Disponible = citaEnHorario == null,
                    CitaId = citaEnHorario?.Id
                });
            }

            var disponibilidad = new DisponibilidadDiaDto
            {
                Fecha = fecha.Date,
                Horarios = horarios
            };

            return Ok(disponibilidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener disponibilidad del día {Fecha}", fecha);
            return StatusCode(500, new { message = "Error al obtener disponibilidad", error = ex.Message });
        }
    }

    // GET: api/Citas/Disponibilidad/Mes?fecha=2024-01-15
    [HttpGet("Disponibilidad/Mes")]
    [AllowAnonymous]
    public async Task<ActionResult<DisponibilidadMesDto>> GetDisponibilidadMes([FromQuery] DateTime fecha)
    {
        try
        {
            var primerDiaMes = new DateTime(fecha.Year, fecha.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            // Obtener todas las citas del mes
            var citasDelMes = await _context.Citas
                .Where(c => c.Fecha >= primerDiaMes && c.Fecha <= ultimoDiaMes.AddDays(1))
                .ToListAsync();

            // Calcular espacios totales por día (8:00 AM - 12:30 PM, cada 30 min = 10 espacios)
            var horaInicio = new TimeSpan(8, 0, 0);
            var horaFin = new TimeSpan(12, 30, 0);
            var intervalo = TimeSpan.FromMinutes(30);
            var totalEspaciosPorDia = (int)((horaFin - horaInicio).TotalMinutes / intervalo.TotalMinutes) + 1;

            var diasDelMes = new List<ResumenDiaDto>();

            // Iterar por cada día del mes
            for (var dia = primerDiaMes; dia <= ultimoDiaMes; dia = dia.AddDays(1))
            {
                // Comparar usando año, mes y día directamente para evitar problemas de zona horaria
                var citasDelDia = citasDelMes.Where(c =>
                    c.Fecha.Year == dia.Year &&
                    c.Fecha.Month == dia.Month &&
                    c.Fecha.Day == dia.Day).ToList();

                // Contar espacios ocupados agrupados por hora
                var espaciosOcupados = 0;
                for (var hora = horaInicio; hora <= horaFin; hora += intervalo)
                {
                    if (citasDelDia.Any(c => c.Fecha.Hour == hora.Hours && c.Fecha.Minute == hora.Minutes))
                    {
                        espaciosOcupados++;
                    }
                }

                var espaciosDisponibles = totalEspaciosPorDia - espaciosOcupados;

                diasDelMes.Add(new ResumenDiaDto
                {
                    Fecha = dia.Date,
                    TotalEspacios = totalEspaciosPorDia,
                    EspaciosOcupados = espaciosOcupados,
                    EspaciosDisponibles = espaciosDisponibles,
                    TieneDisponibilidad = espaciosDisponibles > 0
                });
            }

            var disponibilidad = new DisponibilidadMesDto
            {
                Anio = fecha.Year,
                Mes = fecha.Month,
                Dias = diasDelMes
            };

            return Ok(disponibilidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener disponibilidad del mes {Fecha}", fecha);
            return StatusCode(500, new { message = "Error al obtener disponibilidad del mes", error = ex.Message });
        }
    }
}

