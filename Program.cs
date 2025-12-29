using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using programacion_proyecto_backend.Data;
using programacion_proyecto_backend.Services;
using DotNetEnv;

// Cargar variables de entorno (.env solo en local, en Docker no molesta)
Env.Load();

// Compatibilidad timestamps PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Leer variables de entorno
builder.Configuration.AddEnvironmentVariables();

// Controllers
builder.Services.AddControllers();


// ======================
// JWT CONFIG
// ======================
var jwtKey =
    Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "TuClaveSecretaSuperSeguraQueDebeTenerAlMenos32Caracteres2024!";

var jwtIssuer =
    Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "ProgramacionProyectoBackend";

var jwtAudience =
    Environment.GetEnvironmentVariable("JWT_AUDIENCE")
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


// ======================
// DATABASE CONFIG
// ======================
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

Console.WriteLine($"[DEBUG] DATABASE_CONNECTION_STRING = {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
);

// Seeder
builder.Services.AddScoped<DataSeeder>();


// ======================
// SWAGGER
// ======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ======================
// CORS
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// ======================
// PORT (Coolify / Docker)
// ======================
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();


// ======================
// DB CONNECTION TEST (REAL)
// ======================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        logger.LogInformation("🔍 Probando conexión a PostgreSQL...");
        context.Database.OpenConnection();
        logger.LogInformation("✅ PostgreSQL CONECTADO CORRECTAMENTE");

        // Ejecutar seeder
        var seeder = services.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ ERROR REAL AL CONECTAR CON POSTGRES");
    }
}


// ======================
// MIDDLEWARE
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ IMPORTANTE: quitar HTTPS en Docker/Coolify
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
