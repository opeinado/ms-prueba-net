# CreditPro – Microservicio de Solicitudes de Crédito

CreditPro es un microservicio construido en .NET 9 que gestiona el ciclo de vida de las solicitudes de crédito y registra un historial de auditoría inmutable. La información transaccional se almacena en PostgreSQL y los eventos se replican en DynamoDB para su análisis y trazabilidad en tiempo real.

## Arquitectura

- **Patrón**: Arquitectura Limpia (Domain, Application, Infrastructure, Api).
- **Persistencia transaccional**: PostgreSQL mediante Entity Framework Core.
- **Auditoría/event sourcing**: DynamoDB utilizando AWS SDK.
- **API**: ASP.NET Core Web API con controladores tradicionales y Swagger.
- **Validaciones**: Se realizan en la capa Application, garantizando reglas de negocio coherentes.
- **Pruebas**: `xUnit`, `FluentAssertions` y `Moq` cubren la lógica crítica de negocio (validaciones, cambios de estado, consultas).

```
CreditPro.Api ────────────────┐
   Controllers / Middleware    │
                               │
CreditPro.Application <────────┼──> CreditPro.Infrastructure ──> PostgreSQL / DynamoDB
   DTOs / Services / Rules     │          EF Core + AWS SDK
                               │
CreditPro.Domain ──────────────┘
   Entities / Value Objects
```

## Requisitos previos

- Docker y Docker Compose (recomendado para ejecución local).
- .NET SDK 9.0 si se ejecuta sin contenedores.
- Opcional: AWS CLI (incluido automáticamente en el contenedor `dynamodb-init`).

## Puesta en marcha con Docker

```bash
# Desde la raíz del proyecto
docker compose up --build
```

Servicios que se levantan:

- `api` (`http://localhost:8080`): Microservicio ASP.NET Core.
- `postgres` (`localhost:5432`): Base de datos transaccional.
- `dynamodb` (`http://localhost:8000`): DynamoDB local.
- `dynamodb-init`: Contenedor efímero que crea la tabla `creditpro-audit-events` si no existe.

El API aplica automáticamente las migraciones de EF Core al iniciar.

Para detener los servicios:

```bash
docker compose down
```

## Ejecución local sin contenedores

1. Provisiona PostgreSQL y crea la base de datos `creditpro`.
2. Crea manualmente la tabla `creditpro-audit-events` en DynamoDB (local o AWS). Ejemplo con AWS CLI:
   ```bash
   aws dynamodb create-table \
     --table-name creditpro-audit-events \
     --attribute-definitions AttributeName=applicationId,AttributeType=S AttributeName=timestamp,AttributeType=S \
     --key-schema AttributeName=applicationId,KeyType=HASH AttributeName=timestamp,KeyType=RANGE \
     --billing-mode PAY_PER_REQUEST \
     --endpoint-url http://localhost:8000
   ```
3. Actualiza `CreditPro.Api/appsettings.Development.json` con las cadenas de conexión y credenciales.
4. Aplica la migración de EF Core:
   ```bash
   dotnet tool run dotnet-ef database update \
     --project CreditPro.Infrastructure/CreditPro.Infrastructure.csproj \
     --startup-project CreditPro.Api/CreditPro.Api.csproj
   ```
5. Levanta la API:
   ```bash
   dotnet run --project CreditPro.Api/CreditPro.Api.csproj
   ```

## Endpoints principales

Base URL por defecto: `http://localhost:8080`.

### Crear solicitud

```bash
curl -X POST http://localhost:8080/api/credit-applications \
  -H "Content-Type: application/json" \
  -d '{
        "customerId": "cust-001",
        "creditAmount": 12000,
        "applicationDate": "2025-01-10T10:30:00Z",
        "collateralDescription": "Vehículo Subaru 2023"
      }'
```

### Actualizar estado

```bash
curl -X PATCH http://localhost:8080/api/credit-applications/{applicationId}/status \
  -H "Content-Type: application/json" \
  -d '{
        "newStatus": "En Análisis",
        "notes": "Se requiere documentación adicional"
      }'
```

Estados válidos: `Aprobada`, `Rechazada`, `En Análisis`.

### Consultar solicitud + historial

```bash
curl http://localhost:8080/api/credit-applications/{applicationId}
```

## Pruebas automatizadas

```bash
dotnet test
```

Las pruebas cubren validaciones críticas del servicio de negocio (`CreditApplicationService`):
- Rangos de monto válidos.
- Transiciones de estado válidas.
- Manejo de solicitudes inexistentes.
- Transformación de datos para respuestas y eventos de auditoría.

### Script de verificación end-to-end

El script `scripts/test_creditpro.sh` automatiza la validación completa de los requisitos:

```bash
bash scripts/test_creditpro.sh
```

Lo que realiza:
- Compila la solución y ejecuta las pruebas unitarias.
- Levanta PostgreSQL y DynamoDB con Docker Compose.
- Inicia la API en modo desarrollo.
- Valida que montos inválidos respondan con HTTP 400.
- Crea una solicitud válida, actualiza su estado y comprueba el historial de auditoría.

Requiere que Docker Desktop esté activo y disponible.

## Scripts y utilidades

- `docker-compose.yml`: Orquestación de API, PostgreSQL y DynamoDB local.
- `Dockerfile`: Imagen reproducible de la API con publicación en modo Release.
- `.config/dotnet-tools.json`: Manifiesto de herramientas locales (`dotnet-ef`).

## Próximos pasos sugeridos

- Añadir autenticación/autorización (JWT/API Key).
- Publicar métricas y health checks (`/healthz`).
- Extender pruebas con escenarios de integración (utilizando contenedores de prueba).

---

Hecho con dedicación para gestionar créditos de manera segura y trazable.
