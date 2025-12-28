# Configuración de Puertos

## Puertos Configurados

El backend está configurado para usar los siguientes puertos **FIJOS**:

| Protocolo | Puerto | URL                          |
|-----------|--------|------------------------------|
| HTTP      | 5148   | http://localhost:5148        |
| HTTPS     | 7148   | https://localhost:7148       |

## URLs Importantes

### Swagger UI
- http://localhost:5148/swagger
- https://localhost:7148/swagger

### API Base
- http://localhost:5148/api
- https://localhost:7148/api

### Health Check
- http://localhost:5148/api/health
- https://localhost:7148/api/health

## Endpoints Principales

### Pacientes
- `GET http://localhost:5148/api/pacientes`
- `POST http://localhost:5148/api/pacientes`
- `GET http://localhost:5148/api/pacientes/{id}`
- `GET http://localhost:5148/api/pacientes/telefono/{telefono}`
- `PUT http://localhost:5148/api/pacientes/{id}`
- `DELETE http://localhost:5148/api/pacientes/{id}`

### Citas
- `GET http://localhost:5148/api/citas`
- `POST http://localhost:5148/api/citas`
- `GET http://localhost:5148/api/citas/{id}`
- `GET http://localhost:5148/api/citas/paciente/{pacienteId}`
- `PUT http://localhost:5148/api/citas/{id}`
- `DELETE http://localhost:5148/api/citas/{id}`

### Estados
- `GET http://localhost:5148/api/estados`
- `GET http://localhost:5148/api/estados/{id}`

## Configuración Frontend

Si estás desarrollando un frontend, usa la siguiente URL base:

```javascript
// Para desarrollo local
const API_BASE_URL = 'http://localhost:5148/api';

// Ejemplo de uso
fetch(`${API_BASE_URL}/estados`)
  .then(response => response.json())
  .then(data => console.log(data));
```

## Archivos de Configuración

Los puertos están configurados en:

1. **Properties/launchSettings.json** - Configuración de desarrollo
2. **appsettings.json** - Configuración de Kestrel (servidor web)

## Ejecutar el Proyecto

```bash
dotnet run
```

O para ejecutar con perfil específico:

```bash
# Solo HTTP
dotnet run --launch-profile http

# HTTPS + HTTP (por defecto)
dotnet run --launch-profile https
```

## Notas

- El puerto **5148** está fijo y no cambiará al reiniciar el proyecto
- El puerto HTTPS **7148** también está fijo
- Swagger se abrirá automáticamente en el navegador al ejecutar el proyecto
- CORS está configurado para permitir todas las origenes (`AllowAll`) durante desarrollo

