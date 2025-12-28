# Data Seeder - Usuario y Datos de Prueba

## Descripción

El seeder se ejecuta automáticamente al iniciar la aplicación si la base de datos está vacía. Crea datos iniciales necesarios para el funcionamiento del sistema.

## Datos Creados

### 1. Roles
- **Administrador** - Acceso completo al sistema
- **Recepcionista** - Gestión de citas y pacientes
- **Médico** - Acceso a información médica

### 2. Usuario Administrador por Defecto

**Credenciales:**
- **Usuario:** `admin`
- **Contraseña:** `admin123`

⚠️ **IMPORTANTE:** Cambia la contraseña del usuario administrador después del primer inicio de sesión en producción.

### 3. Estados de Cita
- Pendiente
- Confirmada
- Completada
- Cancelada
- No Asistió

### 4. Datos de Prueba (Opcional)

**Pacientes de prueba:**
- Juan Pérez - Tel: 3001234567
- María García - Tel: 3002345678
- Carlos Rodríguez - Tel: 3003456789

**Citas de prueba:**
- 2 citas pendientes para los primeros pacientes

## Funcionamiento

### Ejecución Automática

El seeder se ejecuta automáticamente cuando:
1. La aplicación inicia
2. Se verifica la conexión a la base de datos
3. **NO** hay roles en la base de datos (primera vez)

Si ya existen datos, el seeder se omite automáticamente.

### Logs

El seeder registra información en los logs:
```
✅ Conexión exitosa a PostgreSQL en {Host}
Iniciando seeder de datos...
Roles creados: 3
Usuario administrador creado: admin / admin123
Estados de cita creados: 5
Pacientes de prueba creados: 3
Citas de prueba creadas: 2
Seeder completado exitosamente.
```

## Verificar que Funcionó

### 1. Verificar Usuario Admin

```bash
# Login con el usuario admin
curl -X POST http://localhost:5148/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"nombre":"admin","clave":"admin123"}'
```

Deberías recibir un token JWT.

### 2. Verificar Roles

```bash
# Listar roles (requiere autenticación)
curl http://localhost:5148/api/roles \
  -H "Authorization: Bearer {token}"
```

### 3. Verificar Estados

```bash
# Listar estados (requiere autenticación)
curl http://localhost:5148/api/estados \
  -H "Authorization: Bearer {token}"
```

### 4. Verificar Pacientes de Prueba

```bash
# Listar pacientes (requiere autenticación)
curl http://localhost:5148/api/pacientes \
  -H "Authorization: Bearer {token}"
```

## Ejecutar Seeder Manualmente

Si necesitas ejecutar el seeder manualmente o reiniciar los datos:

### Opción 1: Eliminar y Recrear

```bash
# Eliminar todas las tablas (¡CUIDADO! Esto borra todos los datos)
dotnet ef database drop

# Recrear la base de datos
dotnet ef database update

# Iniciar la aplicación (el seeder se ejecutará automáticamente)
dotnet run
```

### Opción 2: Crear un Endpoint de Seeder (Solo Desarrollo)

Puedes agregar un endpoint temporal para ejecutar el seeder:

```csharp
[HttpPost("seed")]
[AllowAnonymous] // Solo en desarrollo
public async Task<IActionResult> Seed()
{
    var seeder = new DataSeeder(_context, _logger);
    await seeder.SeedAsync();
    return Ok(new { message = "Seeder ejecutado exitosamente" });
}
```

## Personalizar el Seeder

Para modificar los datos que se crean, edita el archivo `Services/DataSeeder.cs`:

### Cambiar Credenciales del Admin

```csharp
var usuarioAdmin = new Usuario
{
    Id = Guid.NewGuid(),
    Nombre = "tu_usuario",  // Cambiar aquí
    Clave = BCrypt.Net.BCrypt.HashPassword("tu_contraseña"), // Cambiar aquí
    RolId = rolAdmin.Id
};
```

### Agregar Más Roles

```csharp
var roles = new[]
{
    new Rol { Id = Guid.NewGuid(), Nombre = "Administrador" },
    new Rol { Id = Guid.NewGuid(), Nombre = "Recepcionista" },
    new Rol { Id = Guid.NewGuid(), Nombre = "Médico" },
    new Rol { Id = Guid.NewGuid(), Nombre = "NuevoRol" } // Agregar aquí
};
```

### Agregar Más Datos de Prueba

Modifica los métodos `SeedDatosPruebaAsync()` para agregar más pacientes, citas, etc.

## Seguridad

⚠️ **IMPORTANTE para Producción:**

1. **Cambiar contraseña del admin** inmediatamente después del primer login
2. **Deshabilitar el seeder** en producción o hacerlo más seguro
3. **No exponer** el endpoint de seeder en producción
4. **Usar variables de entorno** para credenciales sensibles

### Deshabilitar Seeder en Producción

Puedes modificar `Program.cs` para que solo se ejecute en desarrollo:

```csharp
if (app.Environment.IsDevelopment())
{
    var seeder = services.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}
```

## Troubleshooting

### El seeder no se ejecuta

1. Verifica que la base de datos esté vacía (sin roles)
2. Revisa los logs de la aplicación
3. Verifica la conexión a la base de datos

### Error al crear usuario

1. Verifica que el rol "Administrador" se haya creado primero
2. Verifica que no exista ya un usuario con el nombre "admin"
3. Revisa los logs para ver el error específico

### Quiero reiniciar los datos

```bash
# Eliminar todas las tablas
dotnet ef database drop --force

# Recrear
dotnet ef database update

# Reiniciar aplicación
dotnet run
```

