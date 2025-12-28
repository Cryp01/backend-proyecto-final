using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Models;
using BCrypt.Net;

namespace programacion_proyecto_backend.Services;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Verificar si ya hay datos
            if (await _context.Roles.AnyAsync())
            {
                _logger.LogInformation("La base de datos ya tiene datos. Seeder omitido.");
                return;
            }

            _logger.LogInformation("Iniciando seeder de datos...");

            // 1. Crear Roles
            await SeedRolesAsync();

            // 2. Crear Usuario Administrador por defecto
            await SeedUsuarioAdminAsync();

            // 3. Crear Estados de Cita
            await SeedEstadosCitaAsync();

            // 4. Crear Datos de Prueba (opcional)
            await SeedDatosPruebaAsync();

            _logger.LogInformation("Seeder completado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar el seeder");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new Rol { Id = Guid.NewGuid(), Nombre = "Administrador" },
            new Rol { Id = Guid.NewGuid(), Nombre = "Recepcionista" },
            new Rol { Id = Guid.NewGuid(), Nombre = "Médico" }
        };

        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Roles creados: {Count}", roles.Length);
    }

    private async Task SeedUsuarioAdminAsync()
    {
        var rolAdmin = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador");
        if (rolAdmin == null)
        {
            _logger.LogWarning("No se encontró el rol Administrador. No se creará el usuario admin.");
            return;
        }

        var usuarioAdmin = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "admin",
            Clave = BCrypt.Net.BCrypt.HashPassword("admin123"), // Contraseña por defecto: admin123
            RolId = rolAdmin.Id
        };

        _context.Usuarios.Add(usuarioAdmin);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario administrador creado: admin / admin123");
    }

    private async Task SeedEstadosCitaAsync()
    {
        // Verificar si ya existen estados
        if (await _context.EstadosCita.AnyAsync())
        {
            return;
        }

        var estados = new[]
        {
            new EstadoCita { Id = Guid.NewGuid(), Nombre = "Pendiente" },
            new EstadoCita { Id = Guid.NewGuid(), Nombre = "Confirmada" },
            new EstadoCita { Id = Guid.NewGuid(), Nombre = "Completada" },
            new EstadoCita { Id = Guid.NewGuid(), Nombre = "Cancelada" },
            new EstadoCita { Id = Guid.NewGuid(), Nombre = "No Asistió" }
        };

        _context.EstadosCita.AddRange(estados);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Estados de cita creados: {Count}", estados.Length);
    }

    private async Task SeedDatosPruebaAsync()
    {
        // Crear algunos pacientes de prueba
        var pacientes = new[]
        {
            new Paciente
            {
                Id = Guid.NewGuid(),
                Nombre = "Juan Pérez",
                Telefono = 3001234567,
                Direccion = "Calle 123 #45-67",
                FechaNacimiento = new DateTime(1990, 5, 15)
            },
            new Paciente
            {
                Id = Guid.NewGuid(),
                Nombre = "María García",
                Telefono = 3002345678,
                Direccion = "Avenida Principal #12-34",
                FechaNacimiento = new DateTime(1985, 8, 20)
            },
            new Paciente
            {
                Id = Guid.NewGuid(),
                Nombre = "Carlos Rodríguez",
                Telefono = 3003456789,
                Direccion = "Carrera 56 #78-90",
                FechaNacimiento = new DateTime(1992, 3, 10)
            }
        };

        _context.Pacientes.AddRange(pacientes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Pacientes de prueba creados: {Count}", pacientes.Length);

        // Crear algunas citas de prueba (opcional)
        var estadoPendiente = await _context.EstadosCita.FirstOrDefaultAsync(e => e.Nombre == "Pendiente");
        if (estadoPendiente != null && pacientes.Length > 0)
        {
            var citas = new[]
            {
                new Cita
                {
                    Id = Guid.NewGuid(),
                    PacienteId = pacientes[0].Id,
                    Fecha = DateTime.Now.AddDays(1).Date.AddHours(9), // Mañana a las 9 AM
                    EstadoId = estadoPendiente.Id,
                    NotaMedica = "Primera consulta"
                },
                new Cita
                {
                    Id = Guid.NewGuid(),
                    PacienteId = pacientes[1].Id,
                    Fecha = DateTime.Now.AddDays(2).Date.AddHours(10).AddMinutes(30), // Pasado mañana a las 10:30 AM
                    EstadoId = estadoPendiente.Id,
                    NotaMedica = "Control"
                }
            };

            _context.Citas.AddRange(citas);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Citas de prueba creadas: {Count}", citas.Length);
        }
    }
}

