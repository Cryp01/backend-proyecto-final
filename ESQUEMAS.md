# Esquemas de Base de Datos

> **Nota:** Las tablas y columnas en PostgreSQL se crean automáticamente en **snake_case** (minúsculas con guiones bajos).

## Tabla: estado_cita

| Campo  | Tipo | Descripción              |
|--------|------|--------------------------|
| id     | uuid | Identificador único      |
| nombre | text | Nombre del estado        |

### Estados disponibles:

El sistema utiliza los estados que ya existen en tu base de datos. Para crear una cita sin especificar estado, el sistema buscará automáticamente el estado con nombre **"Pendiente"**.

Los estados comunes son:
- `Pendiente` - **Estado por defecto** (se asigna automáticamente si no se especifica)
- `Confirmada`
- `Completada`
- `Cancelada`
- `No Asistió`

Para obtener la lista completa de estados con sus IDs, consulta el endpoint: `GET /api/estados`

---

## Tabla: pacientes

| Campo            | Tipo         | Descripción              | Requerido |
|------------------|--------------|--------------------------|-----------|
| id               | uuid         | Identificador único      | Sí        |
| nombre           | text         | Nombre del paciente      | Sí        |
| telefono         | numeric(10)  | Teléfono del paciente    | No        |
| direccion        | text         | Dirección del paciente   | No        |
| fecha_nacimiento | date         | Fecha de nacimiento      | No        |

---

## Tabla: roles

| Campo | Tipo | Descripción         | Requerido |
|-------|------|---------------------|-----------|
| id    | uuid | Identificador único | Sí        |
| nombre| text | Nombre del rol      | Sí        |

---

## Tabla: usuarios

| Campo  | Tipo | Descripción                      | Requerido |
|--------|------|----------------------------------|-----------|
| id     | uuid | Identificador único              | Sí        |
| nombre | text | Nombre de usuario (único)        | Sí        |
| clave  | text | Contraseña encriptada (BCrypt)   | Sí        |
| rol_id | uuid | FK a roles                       | Sí        |

---

## Tabla: citas

| Campo          | Tipo                    | Descripción                  | Requerido |
|----------------|-------------------------|------------------------------|-----------|
| id             | uuid                    | Identificador único          | Sí        |
| paciente_id    | uuid                    | FK a pacientes               | Sí        |
| fecha          | timestamp without tz    | Fecha y hora de la cita      | Sí        |
| estado_id      | uuid                    | FK a estado_cita             | Sí        |
| nota_medica    | text                    | Notas médicas de la cita     | No        |

### Relaciones:
- `paciente_id` → `pacientes.id` (CASCADE on delete)
- `estado_id` → `estado_cita.id` (RESTRICT on delete)

---

## URL Base del API

- **HTTP:** `http://localhost:5148`
- **HTTPS:** `https://localhost:7148`
- **Swagger:** `http://localhost:5148/swagger` o `https://localhost:7148/swagger`

## 🔓 Endpoints Públicos vs 🔒 Protegidos

### Endpoints Públicos (No requieren autenticación):
- Disponibilidad (día y mes)
- Crear citas
- Crear pacientes
- Consultar paciente (ID y teléfono)
- Consultar citas por pacienteId
- Login

### Endpoints Protegidos (Requieren token JWT):
- Todos los demás endpoints de gestión y administración

Ver `ENDPOINTS_AUTORIZACION.md` para más detalles.

---

## Endpoints API Disponibles

### Pacientes
- `GET /api/pacientes` - Listar todos los pacientes
- `GET /api/pacientes/{id}` - Obtener un paciente por ID
- `GET /api/pacientes/telefono/{telefono}` - Obtener un paciente por teléfono
- `POST /api/pacientes` - Crear un nuevo paciente
- `PUT /api/pacientes/{id}` - Actualizar un paciente
- `DELETE /api/pacientes/{id}` - Eliminar un paciente

### Citas
- `GET /api/citas` - Listar todas las citas
- `GET /api/citas/{id}` - Obtener una cita por ID
- `GET /api/citas/paciente/{pacienteId}` - Listar citas de un paciente
- `GET /api/citas/disponibilidad/dia?fecha=2024-01-15` - Ver disponibilidad de un día específico
- `GET /api/citas/disponibilidad/mes?fecha=2024-01-15` - Ver disponibilidad de todo el mes
- `POST /api/citas` - Crear una nueva cita
- `PUT /api/citas/{id}` - Actualizar una cita
- `DELETE /api/citas/{id}` - Eliminar una cita

### Estados
- `GET /api/estados` - Listar todos los estados disponibles
- `GET /api/estados/{id}` - Obtener un estado por ID

### Autenticación
- `POST /api/auth/login` - Iniciar sesión y obtener token JWT

### Usuarios
- `GET /api/usuarios` - Listar todos los usuarios
- `GET /api/usuarios/{id}` - Obtener un usuario por ID
- `POST /api/usuarios` - Crear un nuevo usuario
- `PUT /api/usuarios/{id}` - Actualizar un usuario
- `DELETE /api/usuarios/{id}` - Eliminar un usuario

### Roles
- `GET /api/roles` - Listar todos los roles
- `GET /api/roles/{id}` - Obtener un rol por ID

### Health Check
- `GET /api/health` - Verificar estado de la API y conexión a BD

---

## Ejemplos de uso

### Crear un paciente
```json
POST /api/pacientes
{
  "nombre": "Juan Pérez",
  "telefono": 3001234567,
  "direccion": "Calle 123 #45-67",
  "fechaNacimiento": "1990-05-15"
}
```

### Buscar un paciente por teléfono
```http
GET /api/pacientes/telefono/3001234567
```

Respuesta:
```json
{
  "id": "guid-del-paciente",
  "nombre": "Juan Pérez",
  "telefono": 3001234567,
  "direccion": "Calle 123 #45-67",
  "fechaNacimiento": "1990-05-15",
  "totalCitas": 5
}
```

### Crear una cita
```json
POST /api/citas
{
  "pacienteId": "guid-del-paciente",
  "fecha": "2024-01-15T10:30:00",
  "estadoId": "00000000-0000-0000-0000-000000000002",  // Opcional, por defecto es "Pendiente"
  "notaMedica": "Primera consulta"
}
```

**Notas:**
- El campo `estadoId` es opcional. Si no se proporciona, el sistema buscará automáticamente el estado "Pendiente" en la base de datos y lo asignará a la cita.
- **Validación:** Un paciente solo puede tener **una cita pendiente** a la vez. Si intenta crear una cita con estado "Pendiente" y ya tiene una cita pendiente, se devolverá un error 400.

**Error si ya tiene cita pendiente:**
```json
{
  "message": "El paciente ya tiene una cita pendiente. Solo puede tener una cita pendiente a la vez."
}
```

**Ejemplo sin especificar estado:**
```json
POST /api/citas
{
  "pacienteId": "guid-del-paciente",
  "fecha": "2024-01-15T10:30:00",
  "notaMedica": "Primera consulta"
}
// Se creará con estado "Pendiente" automáticamente
```

### Verificar disponibilidad de un día específico
```http
GET /api/citas/disponibilidad/dia?fecha=2024-01-15
```

Respuesta:
```json
{
  "fecha": "2024-01-15T00:00:00",
  "horarios": [
    {
      "hora": "08:00:00",
      "horaFormateada": "08:00",
      "disponible": true,
      "citaId": null
    },
    {
      "hora": "08:30:00",
      "horaFormateada": "08:30",
      "disponible": false,
      "citaId": "guid-de-la-cita"
    },
    {
      "hora": "09:00:00",
      "horaFormateada": "09:00",
      "disponible": true,
      "citaId": null
    }
    // ... hasta 12:30 PM
  ]
}
```

### Ver disponibilidad del mes
```http
GET /api/citas/disponibilidad/mes?fecha=2024-01-15
```

Respuesta:
```json
{
  "anio": 2024,
  "mes": 1,
  "dias": [
    {
      "fecha": "2024-01-01T00:00:00",
      "totalEspacios": 10,
      "espaciosOcupados": 3,
      "espaciosDisponibles": 7,
      "tieneDisponibilidad": true
    },
    {
      "fecha": "2024-01-02T00:00:00",
      "totalEspacios": 10,
      "espaciosOcupados": 10,
      "espaciosDisponibles": 0,
      "tieneDisponibilidad": false
    }
    // ... todos los días del mes
  ]
}
```

### Actualizar una cita
```json
PUT /api/citas/{id}
{
  "pacienteId": "guid-del-paciente",
  "fecha": "2024-01-15T11:00:00",
  "estadoId": "00000000-0000-0000-0000-000000000002",
  "notaMedica": "Cita confirmada"
}
```

**Nota:** Si se intenta cambiar el estado de una cita a "Pendiente" y el paciente ya tiene otra cita pendiente, se devolverá un error 400.

### Login (Autenticación)
```http
POST /api/auth/login
Content-Type: application/json

{
  "nombre": "admin",
  "clave": "password123"
}
```

Respuesta exitosa:
```json
{
  "usuario": {
    "id": "guid-del-usuario",
    "nombre": "admin",
    "rolId": "guid-del-rol",
    "rolNombre": "Administrador"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Uso del token:**
Incluye el token en el header de las peticiones:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Crear un usuario
```json
POST /api/usuarios
{
  "nombre": "usuario1",
  "clave": "password123",
  "rolId": "guid-del-rol"
}
```

**Nota:** La contraseña se encripta automáticamente con BCrypt antes de guardarse.

### Actualizar un usuario
```json
PUT /api/usuarios/{id}
{
  "nombre": "usuario1",
  "clave": "nuevaPassword123",  // Opcional, solo si se desea cambiar
  "rolId": "guid-del-rol"
}
```

**Nota:** El campo `clave` es opcional. Si no se proporciona, la contraseña no se modifica.

### Obtener todos los estados
```http
GET /api/estados
```

Respuesta (ejemplo):
```json
[
  {
    "id": "6024faa7-6e5f-4093-ac65-89da3fab9842",
    "nombre": "Pendiente"
  },
  {
    "id": "guid-confirmada",
    "nombre": "Confirmada"
  },
  {
    "id": "guid-completada",
    "nombre": "Completada"
  },
  {
    "id": "guid-cancelada",
    "nombre": "Cancelada"
  },
  {
    "id": "guid-no-asistio",
    "nombre": "No Asistió"
  }
]
```

> **Nota:** Los IDs exactos dependen de tu base de datos. Usa este endpoint para obtener los IDs reales.

---

## Migraciones

Para crear y aplicar las migraciones a la base de datos:

```bash
# Crear la migración inicial
dotnet ef migrations add InitialCreate

# Aplicar las migraciones a la base de datos
dotnet ef database update
```

Para verificar las migraciones:
```bash
dotnet ef migrations list
```

