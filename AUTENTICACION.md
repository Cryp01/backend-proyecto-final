# Sistema de Autenticación

## Descripción General

El sistema de autenticación utiliza **JWT (JSON Web Tokens)** para autenticar usuarios. Las contraseñas se almacenan encriptadas usando **BCrypt**.

## Esquema de Base de Datos

### Tabla: `roles`

| Campo | Tipo | Descripción         |
|-------|------|---------------------|
| id    | uuid | Identificador único |
| nombre| text | Nombre del rol      |

### Tabla: `usuarios`

| Campo  | Tipo | Descripción                    |
|--------|------|--------------------------------|
| id     | uuid | Identificador único            |
| nombre | text | Nombre de usuario (único)      |
| clave  | text | Contraseña encriptada (BCrypt) |
| rol_id | uuid | FK a roles                     |

## Endpoints

### 1. Login

**POST** `/api/auth/login`

**Body:**
```json
{
  "nombre": "usuario",
  "clave": "contraseña"
}
```

**Respuesta exitosa (200):**
```json
{
  "usuario": {
    "id": "guid-del-usuario",
    "nombre": "usuario",
    "rolId": "guid-del-rol",
    "rolNombre": "Administrador"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Respuesta de error (401):**
```json
{
  "message": "Usuario o contraseña incorrectos"
}
```

### 2. Gestión de Usuarios

#### Listar usuarios
**GET** `/api/usuarios`

#### Obtener usuario
**GET** `/api/usuarios/{id}`

#### Crear usuario
**POST** `/api/usuarios`
```json
{
  "nombre": "nuevoUsuario",
  "clave": "contraseña123",
  "rolId": "guid-del-rol"
}
```

#### Actualizar usuario
**PUT** `/api/usuarios/{id}`
```json
{
  "nombre": "usuario",
  "clave": "nuevaContraseña",  // Opcional
  "rolId": "guid-del-rol"
}
```

#### Eliminar usuario
**DELETE** `/api/usuarios/{id}`

### 3. Gestión de Roles

#### Listar roles
**GET** `/api/roles`

#### Obtener rol
**GET** `/api/roles/{id}`

## Configuración JWT

La configuración de JWT se encuentra en `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "TuClaveSecretaSuperSeguraQueDebeTenerAlMenos32Caracteres2024!",
    "Issuer": "ProgramacionProyectoBackend",
    "Audience": "ProgramacionProyectoBackend",
    "ExpiryMinutes": "1440"
  }
}
```

### Parámetros:

- **Key:** Clave secreta para firmar los tokens (debe tener al menos 32 caracteres)
- **Issuer:** Emisor del token
- **Audience:** Audiencia del token
- **ExpiryMinutes:** Tiempo de expiración del token en minutos (1440 = 24 horas)

## Uso del Token

Una vez obtenido el token del endpoint de login, inclúyelo en todas las peticiones que requieran autenticación:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Ejemplo con cURL:

```bash
curl -H "Authorization: Bearer TOKEN_AQUI" \
     http://localhost:5148/api/usuarios
```

### Ejemplo con JavaScript:

```javascript
const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

fetch('http://localhost:5148/api/usuarios', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

## Seguridad

### Contraseñas

- Las contraseñas se encriptan usando **BCrypt** antes de guardarse en la base de datos
- BCrypt genera un hash único incluso para la misma contraseña
- El costo de encriptación es configurable (por defecto 11 rondas)

### Tokens JWT

- Los tokens incluyen información del usuario (ID, nombre, rol)
- Los tokens expiran después del tiempo configurado (por defecto 24 horas)
- Los tokens son firmados con una clave secreta
- El token no se almacena en la base de datos

### Claims del Token

El token JWT incluye los siguientes claims:

- `nameid`: ID del usuario (Guid)
- `name`: Nombre del usuario
- `role`: Nombre del rol
- `RolId`: ID del rol (claim personalizado)

## Protección de Endpoints

Para proteger un endpoint y requerir autenticación, usa el atributo `[Authorize]`:

```csharp
[Authorize]
[HttpGet]
public async Task<ActionResult> GetData()
{
    // Solo usuarios autenticados pueden acceder
}
```

Para requerir un rol específico:

```csharp
[Authorize(Roles = "Administrador")]
[HttpPost]
public async Task<ActionResult> CreateData()
{
    // Solo administradores pueden acceder
}
```

## Creación de Roles Iniciales

Para crear roles iniciales, puedes usar migraciones con datos seed o crearlos directamente en la base de datos.

Ejemplo de roles comunes:
- Administrador
- Recepcionista
- Médico
- Usuario

## Recomendaciones de Seguridad

1. ⚠️ **NUNCA** expongas la clave JWT en el código fuente en producción
2. ⚠️ Usa variables de entorno para la clave JWT en producción
3. ✅ Usa HTTPS en producción
4. ✅ Configura un tiempo de expiración razonable para los tokens
5. ✅ Implementa renovación de tokens si es necesario
6. ✅ Considera implementar refresh tokens para mayor seguridad
7. ✅ Valida y sanitiza todas las entradas del usuario
8. ✅ Usa contraseñas seguras (mínimo 8 caracteres, mayúsculas, minúsculas, números)

## Migraciones

Para crear las tablas en la base de datos:

```bash
# Crear migración
dotnet ef migrations add AddUsuariosAndRoles

# Aplicar migración
dotnet ef database update
```

