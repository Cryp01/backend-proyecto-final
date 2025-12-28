# Proyecto Backend - Programación

API REST construida con ASP.NET Core 7.0 y PostgreSQL.

## Requisitos Previos

- .NET 7.0 SDK
- PostgreSQL 12 o superior
- Visual Studio Code o Visual Studio 2022

## Configuración

### 1. Variables de Entorno

El proyecto usa variables de entorno para configuraciones sensibles. Crea un archivo `.env` en la raíz del proyecto:

```bash
# Copiar el archivo de ejemplo
cp .env.example .env

# Editar con tus valores
nano .env  # o usa tu editor preferido
```

**Variables requeridas:**
- `DATABASE_CONNECTION_STRING` - Cadena de conexión a PostgreSQL
- `JWT_KEY` - Clave secreta para JWT (mínimo 32 caracteres)

Ver `VARIABLES_ENTORNO.md` para más detalles.

### 2. Base de Datos

Asegúrate de que PostgreSQL esté instalado y en ejecución. Configura la cadena de conexión en el archivo `.env`:

```env
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=programacion_proyecto_db;Username=postgres;Password=postgres
```

### 2. Crear la Base de Datos

```sql
CREATE DATABASE programacion_proyecto_db;
```

### 3. Restaurar Paquetes

```bash
dotnet restore
```

### 4. Aplicar Migraciones

Cuando crees tus modelos, ejecuta:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Ejecutar el Proyecto

```bash
dotnet run
```

La API estará disponible en:
- **HTTP:** http://localhost:5148
- **HTTPS:** https://localhost:7148

Swagger UI estará disponible en:
- http://localhost:5148/swagger
- https://localhost:7148/swagger

## Estructura del Proyecto

```
programacion-proyecto-backend/
├── Controllers/        # Controladores de la API
├── Data/              # Contexto de base de datos
├── Models/            # Modelos de datos/entidades
├── Program.cs         # Punto de entrada de la aplicación
└── appsettings.json   # Configuración de la aplicación
```

## Paquetes Instalados

- **Npgsql.EntityFrameworkCore.PostgreSQL** - Proveedor de PostgreSQL para Entity Framework Core
- **Microsoft.EntityFrameworkCore.Design** - Herramientas de diseño de EF Core
- **Microsoft.EntityFrameworkCore.Tools** - Herramientas CLI para migraciones
- **Swashbuckle.AspNetCore** - Generación de documentación Swagger/OpenAPI

## Próximos Pasos

1. Crear tus modelos en la carpeta `Models/`
2. Agregar DbSets en `ApplicationDbContext.cs`
3. Crear controladores en la carpeta `Controllers/`
4. Ejecutar migraciones para crear las tablas en la base de datos

