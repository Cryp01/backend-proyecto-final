# Guía de Migraciones

## ⚠️ IMPORTANTE: Antes de crear migraciones

Este proyecto usa **snake_case naming convention** para PostgreSQL. Esto significa que:
- Las tablas se crean en minúsculas con guiones bajos: `pacientes`, `citas`, `estado_cita`
- Las columnas también: `id`, `nombre`, `paciente_id`, `fecha_nacimiento`, etc.

## Crear y aplicar migraciones

### 1. Verificar que las herramientas de EF Core estén instaladas

```bash
dotnet tool install --global dotnet-ef
```

O actualizar si ya están instaladas:

```bash
dotnet tool update --global dotnet-ef
```

### 2. Crear la migración inicial

```bash
dotnet ef migrations add InitialCreate
```

Esto creará una carpeta `Migrations/` con los archivos de migración.

### 3. Revisar la migración (opcional pero recomendado)

Abre el archivo en `Migrations/[timestamp]_InitialCreate.cs` y verifica que las tablas se creen correctamente:
- `pacientes`
- `citas`
- `estado_cita`

### 4. Aplicar la migración a la base de datos

```bash
dotnet ef database update
```

Este comando:
- Creará las tablas en PostgreSQL
- Insertará los datos iniciales de `estado_cita` (Pendiente, Confirmada, Completada, Cancelada, No Asistió)

### 5. Verificar que las tablas se crearon

Puedes conectarte a PostgreSQL con pgAdmin o desde la terminal:

```bash
psql -U postgres -d programacion_proyecto_db
```

Y ejecutar:

```sql
\dt
```

Deberías ver:
- `pacientes`
- `citas`
- `estado_cita`
- `__EFMigrationsHistory` (tabla interna de EF Core)

Para ver las columnas de una tabla:

```sql
\d pacientes
\d citas
\d estado_cita
```

### 6. Verificar los datos iniciales

```sql
SELECT * FROM estado_cita;
```

Deberías ver los 5 estados predefinidos.

## Comandos útiles

### Ver lista de migraciones

```bash
dotnet ef migrations list
```

### Crear una nueva migración (después de cambiar modelos)

```bash
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar migraciones pendientes

```bash
dotnet ef database update
```

### Revertir a una migración específica

```bash
dotnet ef database update NombreDeLaMigracionAnterior
```

### Eliminar la última migración (si NO se ha aplicado)

```bash
dotnet ef migrations remove
```

### Generar script SQL de las migraciones

```bash
dotnet ef migrations script
```

O para un rango específico:

```bash
dotnet ef migrations script MigracionInicial MigracionFinal
```

## Solución de problemas

### Error: "A network-related or instance-specific error occurred"

- Verifica que PostgreSQL esté ejecutándose
- Verifica la cadena de conexión en `appsettings.json`
- Prueba la conexión con pgAdmin o psql

### Error: "Column does not exist" o nombres en mayúsculas

- Este proyecto usa `EFCore.NamingConventions` que convierte todo a snake_case
- Las tablas son: `pacientes`, `citas`, `estado_cita` (todo en minúsculas)
- Las columnas son: `id`, `nombre`, `paciente_id`, etc. (todo en minúsculas)

### Error: "Build failed"

Si la aplicación está corriendo y no puedes compilar:
- Detén la aplicación (Ctrl+C)
- Luego ejecuta `dotnet build`

### Si necesitas recrear la base de datos

```bash
# Eliminar todas las migraciones de la BD (cuidado, esto borra todos los datos)
dotnet ef database drop

# Volver a aplicar las migraciones
dotnet ef database update
```

## Flujo de trabajo recomendado

1. Modifica tus modelos en `Models/`
2. Actualiza `ApplicationDbContext.cs` si es necesario
3. Crea una migración: `dotnet ef migrations add DescripcionDelCambio`
4. Revisa los archivos generados en `Migrations/`
5. Aplica la migración: `dotnet ef database update`
6. Verifica en PostgreSQL que los cambios se aplicaron correctamente

