# Instrucciones para configurar la base de datos PostgreSQL

## 1. Instalación de PostgreSQL

Si aún no tienes PostgreSQL instalado:

### Windows
- Descarga desde: https://www.postgresql.org/download/windows/
- O usa Chocolatey: `choco install postgresql`

### Configuración durante la instalación
- Usuario predeterminado: `postgres`
- Contraseña: La que elijas (por defecto en el proyecto: `postgres`)
- Puerto: `5432`

## 2. Crear la base de datos

Abre **pgAdmin** o la terminal de PostgreSQL (`psql`) y ejecuta:

```sql
CREATE DATABASE programacion_proyecto_db;
```

O desde la terminal de Windows:

```bash
psql -U postgres
# Luego dentro de psql:
CREATE DATABASE programacion_proyecto_db;
\q
```

## 3. Configurar la cadena de conexión

Edita `appsettings.json` o `appsettings.Development.json` si necesitas cambiar:
- Host
- Puerto
- Nombre de la base de datos
- Usuario
- Contraseña

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=programacion_proyecto_db;Username=postgres;Password=TU_CONTRASEÑA"
}
```

## 4. Ejecutar el proyecto

```bash
dotnet run
```

## 5. Verificar la conexión

Una vez que el proyecto esté ejecutándose, abre tu navegador:

- Swagger UI: `https://localhost:7xxx/swagger`
- Endpoint de salud: `GET https://localhost:7xxx/api/health`

El endpoint `/api/health` te dirá si la conexión a PostgreSQL está funcionando correctamente.

## 6. Crear modelos y migraciones (cuando estés listo)

### Paso 1: Crea tus modelos en la carpeta `Models/`

Ejemplo - `Models/Usuario.cs`:

```csharp
namespace programacion_proyecto_backend.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
```

### Paso 2: Agrega el DbSet en `Data/ApplicationDbContext.cs`

```csharp
public DbSet<Usuario> Usuarios { get; set; }
```

### Paso 3: Crea y aplica la migración

```bash
# Crear la migración
dotnet ef migrations add InitialCreate

# Aplicar la migración a la base de datos
dotnet ef database update
```

## Comandos útiles de Entity Framework

```bash
# Ver el estado de las migraciones
dotnet ef migrations list

# Crear una nueva migración
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a una migración específica
dotnet ef database update NombreDeLaMigracion

# Eliminar la última migración (si no se ha aplicado)
dotnet ef migrations remove

# Generar script SQL de las migraciones
dotnet ef migrations script
```

## Solución de problemas

### Error: "No se puede conectar a PostgreSQL"

1. Verifica que PostgreSQL esté ejecutándose:
   - Windows: Busca "Services" → `postgresql-x64-xx`
   
2. Verifica la cadena de conexión en `appsettings.json`

3. Prueba la conexión con pgAdmin

### Error: "La base de datos no existe"

Ejecuta el comando SQL para crear la base de datos (paso 2).

### Error al crear migraciones

Asegúrate de tener las herramientas de EF Core instaladas:

```bash
dotnet tool install --global dotnet-ef
```

