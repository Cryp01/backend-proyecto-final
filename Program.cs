using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Services;
using DotNetEnv;

// Cargar variables de entorno desde .env
Env.Load();

// Habilitar comportamiento legacy de timestamps para compatibilidad con PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Agregar variables de entorno a la configuración
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

// Configurar JWT Authentication
var jwtKey = builder.Configuration["JWT_KEY"]
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "TuClaveSecretaSuperSeguraQueDebeTenerAlMenos32Caracteres2024!";

var jwtIssuer = builder.Configuration["JWT_ISSUER"]
    ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "ProgramacionProyectoBackend";

var jwtAudience = builder.Configuration["JWT_AUDIENCE"]
    ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "ProgramacionProyectoBackend";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=programacion_proyecto_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// Registrar DataSeeder
builder.Services.AddScoped<DataSeeder>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure port from environment variable (for Coolify/Docker)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

// Verificar conexión a la base de datos y ejecutar seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        if (canConnect)
        {
            logger.LogInformation("✅ Conexión exitosa a PostgreSQL en {Host}",
                context.Database.GetConnectionString()?.Split(';')[0].Replace("Host=", ""));

            // Ejecutar seeder
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }
        else
        {
            logger.LogWarning("⚠️ No se pudo conectar a la base de datos PostgreSQL");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error al intentar conectar a PostgreSQL: {Message}", ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
