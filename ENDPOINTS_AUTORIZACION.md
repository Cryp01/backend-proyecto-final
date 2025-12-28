# Endpoints - Autorización

## 🔓 Endpoints Públicos (No Requieren Autenticación)

Estos endpoints pueden ser accedidos sin token JWT.

### Disponibilidad
- `GET /api/citas/disponibilidad/dia?fecha=2024-01-15` - Ver disponibilidad de un día específico
- `GET /api/citas/disponibilidad/mes?fecha=2024-01-15` - Ver disponibilidad del mes completo

### Citas
- `POST /api/citas` - Crear una nueva cita
- `GET /api/citas/paciente/{pacienteId}` - Consultar citas por ID de paciente

### Pacientes
- `POST /api/pacientes` - Crear un nuevo paciente
- `GET /api/pacientes/{id}` - Consultar paciente por ID
- `GET /api/pacientes/telefono/{telefono}` - Consultar paciente por teléfono

### Autenticación
- `POST /api/auth/login` - Iniciar sesión (obtener token JWT)

### Health Check
- `GET /api/health` - Verificar estado de la API y conexión a BD

---

## 🔒 Endpoints Protegidos (Requieren Autenticación)

Estos endpoints requieren un token JWT válido en el header `Authorization: Bearer {token}`.

### Citas
- `GET /api/citas` - Listar todas las citas
- `GET /api/citas/{id}` - Obtener una cita por ID
- `PUT /api/citas/{id}` - Actualizar una cita
- `DELETE /api/citas/{id}` - Eliminar una cita

### Pacientes
- `GET /api/pacientes` - Listar todos los pacientes
- `PUT /api/pacientes/{id}` - Actualizar un paciente
- `DELETE /api/pacientes/{id}` - Eliminar un paciente

### Estados
- `GET /api/estados` - Listar todos los estados
- `GET /api/estados/{id}` - Obtener un estado por ID

### Usuarios
- `GET /api/usuarios` - Listar todos los usuarios
- `GET /api/usuarios/{id}` - Obtener un usuario por ID
- `POST /api/usuarios` - Crear un nuevo usuario
- `PUT /api/usuarios/{id}` - Actualizar un usuario
- `DELETE /api/usuarios/{id}` - Eliminar un usuario

### Roles
- `GET /api/roles` - Listar todos los roles
- `GET /api/roles/{id}` - Obtener un rol por ID

---

## 🔑 Cómo Usar la Autenticación

### 1. Obtener Token

```http
POST /api/auth/login
Content-Type: application/json

{
  "nombre": "usuario",
  "clave": "contraseña"
}
```

**Respuesta:**
```json
{
  "usuario": {
    "id": "guid",
    "nombre": "usuario",
    "rolId": "guid",
    "rolNombre": "Administrador"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Usar el Token

Incluye el token en el header `Authorization` de todas las peticiones protegidas:

```http
GET /api/citas
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Ejemplo con cURL

```bash
# Login
curl -X POST http://localhost:5148/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"nombre":"usuario","clave":"contraseña"}'

# Usar el token recibido
curl http://localhost:5148/api/citas \
  -H "Authorization: Bearer TOKEN_AQUI"
```

### 4. Ejemplo con JavaScript (Fetch)

```javascript
// Login
const loginResponse = await fetch('http://localhost:5148/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    nombre: 'usuario',
    clave: 'contraseña'
  })
});

const { token } = await loginResponse.json();

// Usar el token
const response = await fetch('http://localhost:5148/api/citas', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const citas = await response.json();
```

---

## ⚠️ Errores de Autenticación

Si intentas acceder a un endpoint protegido sin token o con token inválido:

**401 Unauthorized:**
```json
{
  "message": "Unauthorized"
}
```

**401 Unauthorized (Token expirado):**
```json
{
  "message": "Token has expired"
}
```

---

## 📝 Notas Importantes

1. ✅ Los endpoints públicos permiten que los clientes externos puedan:
   - Consultar disponibilidad de citas
   - Crear citas
   - Crear pacientes
   - Consultar información básica de pacientes

2. 🔒 Los endpoints protegidos requieren autenticación y están pensados para:
   - Administración del sistema
   - Gestión completa de citas y pacientes
   - Administración de usuarios y roles

3. ⏰ Los tokens JWT expiran después del tiempo configurado (por defecto 24 horas)

4. 🔄 Para continuar usando la API después de que expire el token, el cliente debe hacer login nuevamente

