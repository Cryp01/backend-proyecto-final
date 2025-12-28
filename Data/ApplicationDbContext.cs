using Microsoft.EntityFrameworkCore;
using programacion_proyecto_backend.Models;

namespace programacion_proyecto_backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Cita> Citas { get; set; }
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<EstadoCita> EstadosCita { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Cita
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Fecha)
                .IsRequired();

            // Relación con Paciente
            entity.HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con EstadoCita
            entity.HasOne(c => c.Estado)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.EstadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Paciente
        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nombre)
                .IsRequired();

            entity.Property(p => p.Telefono)
                .HasColumnType("numeric(10)");

            entity.Property(p => p.FechaNacimiento)
                .HasColumnType("date");
        });

        // Configuración de EstadoCita
        modelBuilder.Entity<EstadoCita>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre)
                .IsRequired();
        });

        // Configuración de Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Nombre)
                .IsRequired();

            entity.HasIndex(r => r.Nombre)
                .IsUnique();
        });

        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Nombre)
                .IsRequired();

            entity.Property(u => u.Clave)
                .IsRequired();

            // Relación con Rol
            entity.HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(u => u.Nombre)
                .IsUnique();
        });
    }
}

