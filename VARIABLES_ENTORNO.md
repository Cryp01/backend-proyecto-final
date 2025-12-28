# Variables de Entorno

El proyecto ahora utiliza variables de entorno para configuraciones sensibles. Esto permite mantener las credenciales fuera del código fuente.

## Configuración

### 1. Crear archivo .env

Copia el archivo `.env.example` y renómbralo a `.env`:

```bash
cp .env.example .env
```

### 2. Editar .env

Edita el archivo `.env` con tus valores reales:

```env
# Base de Datos PostgreSQL
DATABASE_CONNECTION_STRING=Host=tu-host;Port=5432;Database=tu-db;Username=tu-usuario;Password=tu-contraseña

# JWT Configuration
JWT_KEY=TuClaveSecretaSuperSeguraQueDebeTenerAlMenos32Caracteres2024!
JWT_ISSUER=ProgramacionProyectoBackend
JWT_AUDIENCE=ProgramacionProyectoBackend
JWT_EXPIRY_MINUTES=1440

# Server Configuration
ASPNETCORE_URLS=http://localhost:5148
```

## Variables Disponibles

### Base de Datos

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DATABASE_CONNECTION_STRING` | Cadena de conexión completa a PostgreSQL | `Host=localhost;Port=5432;Database=db;Username=user;Password=pass` |

**O usar variables individuales:**
- `DB_HOST` - Host de PostgreSQL
- `DB_PORT` - Puerto de PostgreSQL
- `DB_NAME` - Nombre de la base de datos
- `DB_USER` - Usuario de PostgreSQL
- `DB_PASSWORD` - Contraseña de PostgreSQL

### JWT (Autenticación)

| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| `JWT_KEY` | Clave secreta para firmar tokens (mínimo 32 caracteres) | (requerido) |
| `JWT_ISSUER` | Emisor del token | `ProgramacionProyectoBackend` |
| `JWT_AUDIENCE` | Audiencia del token | `ProgramacionProyectoBackend` |
| `JWT_EXPIRY_MINUTES` | Tiempo de expiración en minutos | `1440` (24 horas) |

### Servidor

| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| `ASPNETCORE_URLS` | URLs donde escucha el servidor | `http://localhost:5148` |

## Orden de Prioridad

El sistema busca las variables en el siguiente orden:

1. **Variables de entorno del sistema** (más alta prioridad)
2. **Archivo .env** (cargado automáticamente)
3. **appsettings.json** (valores por defecto)
4. **Valores hardcodeados** (último recurso)

## Ejemplo de .env

```env
# Base de Datos
DATABASE_CONNECTION_STRING=Host=5.161.108.247;Port=5432;Database=programacion_proyecto_db;Username=postgres;Password=tu_contraseña_segura

# JWT - IMPORTANTE: Usa una clave segura en producción
JWT_KEY=TuClaveSecretaSuperSeguraQueDebeTenerAlMenos32Caracteres2024!
JWT_ISSUER=ProgramacionProyectoBackend
JWT_AUDIENCE=ProgramacionProyectoBackend
JWT_EXPIRY_MINUTES=1440

# Servidor
ASPNETCORE_URLS=http://localhost:5148
```

## Seguridad

### ⚠️ IMPORTANTE:

1. **NUNCA** subas el archivo `.env` al repositorio
- Ya está en `.gitignore`
- Contiene información sensible

2. **NUNCA** uses la misma clave JWT en desarrollo y producción

3. **Genera claves seguras** para producción:
   ```bash
   # Generar una clave aleatoria segura (32+ caracteres)
   openssl rand -base64 32
   ```

4. **Usa variables de entorno del sistema** en producción (Docker, servidores, etc.)

## Uso en Docker

Si usas Docker, puedes pasar variables de entorno:

```yaml
# docker-compose.yml
services:
  backend:
    environment:
      - DATABASE_CONNECTION_STRING=Host=db;Port=5432;Database=app_db;Username=postgres;Password=password
      - JWT_KEY=tu-clave-secreta
      - JWT_ISSUER=ProgramacionProyectoBackend
      - JWT_AUDIENCE=ProgramacionProyectoBackend
```

O usar un archivo `.env` en el directorio del proyecto.

## Uso en Producción

En producción, configura las variables de entorno directamente en el sistema:

### Linux/Unix:
```bash
export DATABASE_CONNECTION_STRING="Host=..."
export JWT_KEY="tu-clave-secreta"
```

### Windows:
```powershell
$env:DATABASE_CONNECTION_STRING="Host=..."
$env:JWT_KEY="tu-clave-secreta"
```

### Azure App Service:
Configura las variables en "Configuration" > "Application settings"

### AWS/Heroku:
Usa sus respectivos sistemas de variables de entorno

## Verificar Variables Cargadas

Para verificar que las variables se están cargando correctamente, revisa los logs al iniciar la aplicación. El sistema mostrará si hay problemas con la configuración.

## Troubleshooting

### Las variables no se cargan

1. Verifica que el archivo `.env` esté en la raíz del proyecto
2. Verifica que no tenga espacios alrededor del `=`
3. Verifica que no haya comillas innecesarias
4. Reinicia la aplicación después de cambiar `.env`

### Error de conexión a la base de datos

1. Verifica que `DATABASE_CONNECTION_STRING` esté correctamente formateada
2. Verifica que la base de datos esté accesible
3. Verifica credenciales

### Error de JWT

1. Verifica que `JWT_KEY` tenga al menos 32 caracteres
2. Verifica que no haya espacios en la clave
3. Usa la misma clave en todos los servicios que validen tokens

