# CourierMax API

> Sistema de logística de envíos — REST API con **Clean Architecture** y **.NET 10**

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com) [![Build](https://img.shields.io/badge/Build-Passing-green)]() [![Tests](https://img.shields.io/badge/Tests-59%20passing-brightgreen)]()

---

## Contenido

- [Arquitectura](#arquitectura)
- [Stack Tecnológico](#stack-tecnológico)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Migraciones y Base de Datos](#migraciones-y-base-de-datos)
- [Endpoints Disponibles](#endpoints-disponibles)
- [Ejemplos de Uso (curl)](#ejemplos-de-uso-curl)
- [Reglas de Negocio Implementadas](#reglas-de-negocio-implementadas)
- [Patrones de Diseño](#patrones-de-diseño)
- [Pruebas](#pruebas)

---

## Arquitectura

Se implementó **Clean Architecture** con separación estricta de capas:

```
CourierMax/
├── src/
│   ├── CourierMax.Domain/          ← Núcleo del negocio (sin dependencias externas)
│   │   ├── Entities/               ← Shipment, Vehicle, Driver, City, Distance, ShipmentHistory
│   │   ├── Enums/                  ← ShipmentStatus, ServiceType, PackageType
│   │   ├── Exceptions/             ← DomainException, InvalidStatusTransitionException...
│   │   ├── Interfaces/             ← IUnitOfWork, IBusinessDayCalculator
│   │   │   └── Repositories/       ← IShipmentRepository, IVehicleRepository...
│   │   └── ValueObjects/           ← TrackingCode, PhoneNumber, PackageDimensions
│   │
│   ├── CourierMax.Application/     ← Casos de uso y servicios de aplicación
│   │   ├── Common/                 ← Result<T> Pattern
│   │   ├── DTOs/                   ← Todos los DTOs de entrada y salida
│   │   ├── Mappings/               ← Mapeos manuales (sin AutoMapper)
│   │   ├── Services/               ← ShipmentService, ReportService
│   │   │   └── Tariff/             ← TariffCalculatorService (Strategy Pattern)
│   │   └── Validators/             ← FluentValidation validators
│   │
│   ├── CourierMax.Infrastructure/  ← Persistencia e implementaciones externas
│   │   ├── Persistence/            ← CourierMaxDbContext, UnitOfWork, DbInitializer
│   │   │   ├── Configurations/     ← Fluent API de EF Core + Seed Data
│   │   │   └── Migrations/         ← Migraciones EF Core
│   │   ├── Repositories/           ← Implementaciones concretas de repositorios
│   │   └── Services/               ← BusinessDayCalculator (festivos Colombia 2026)
│   │
│   └── CourierMax.API/             ← Capa de presentación HTTP
│       ├── Controllers/            ← ShipmentsController, ReportsController, VehiclesController
│       ├── Middleware/             ← GlobalExceptionMiddleware (Problem Details RFC 7807)
│       └── Program.cs              ← DI, Pipeline, Rate Limiting, OpenAPI
│
└── tests/
    ├── CourierMax.UnitTests/        ← 59 tests con xUnit + FluentAssertions
    └── CourierMax.IntegrationTests/ ← Tests de endpoints con SQLite in-memory
```

### Decisiones Arquitectónicas

| Decisión | Justificación |
|---|---|
| **Clean Architecture** | Independencia del dominio respecto a frameworks y bases de datos |
| **Sin MediatR** | YAGNI — servicios directos son suficientes y más simples (KISS) |
| **Mapeos manuales** | Evita dependencia de AutoMapper, mayor control y transparencia |
| **Result Pattern** | Control de errores predecible sin `throw` en flujo normal |
| **Strategy Pattern (Tarifas)** | Fácil extensión para nuevos tipos de servicio sin modificar código existente |
| **Specification Pattern** | Consultas complejas (SLA overdue) encapsuladas y reutilizables |
| **SQLite en IntegrationTests** | Pruebas sin depender de SQL Server en CI/CD |
| **Native .NET 10 OpenAPI + Swagger UI** | Solución moderna sin conflictos de versión |

---

## Stack Tecnológico

| Capa | Tecnología |
|---|---|
| Framework | .NET 10 / ASP.NET Core 10 |
| Base de datos | SQL Server + Entity Framework Core 9 |
| Validaciones | FluentValidation 12 |
| Logging | Serilog (Console) |
| Documentación API | Microsoft.AspNetCore.OpenApi + Swagger UI |
| Pruebas Unitarias | xUnit + FluentAssertions + Moq |
| Pruebas Integración | xUnit + SQLite In-Memory |
| Rate Limiting | ASP.NET Core Rate Limiting (nativo) |
| Health Checks | Microsoft.AspNetCore.Diagnostics.HealthChecks |

---

## Instalación y Ejecución

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local o Docker)

### 1. Clonar el repositorio

```bash
git clone https://github.com/yoinerf/couriermax-api.git
cd couriermax-api
```

### 2. Configurar la conexión a SQL Server

Editar `src/CourierMax.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=CourierMax;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Command Timeout=30"
},
}
```

### 3. Aplicar Migraciones o ejecutar API para iniciar migración

```bash
dotnet ef database update --project src/CourierMax.Infrastructure --startup-project src/CourierMax.API
```

> La aplicación también aplica migraciones automáticamente al iniciar si la base de datos existe.

### 4. Ejecutar la API

```bash
dotnet run --project src/CourierMax.API
```

La API estará disponible en:
- **Base URL**: `http://localhost:5041`
- **Swagger UI**: `http://localhost:5041/swagger`
- **OpenAPI JSON**: `http://localhost:5041/openapi/v1.json`
- **Health Check**: `http://localhost:5041/health`

> Si la URL difiere, revisa `src/CourierMax.API/Properties/launchSettings.json` o los mensajes de arranque.

---

## Datos de Referencia Pre-cargados

Al iniciar, la base de datos se inicializa con:

**Ciudades:** Bogotá (1), Medellín (2), Cali (3), Barranquilla (4)

**Rutas y Tarifas:**

| Origen | Destino | Km | Tarifa |
|---|---|---|---|
| Bogotá | Medellín | 480 | $12,000 |
| Bogotá | Cali | 360 | $9,000 |
| Bogotá | Barranquilla | 950 | $20,000 |
| Medellín | Cali | 310 | $8,000 |
| Medellín | Barranquilla | 650 | $15,000 |
| Cali | Barranquilla | 900 | $18,000 |

**Vehículos y Conductores:**

| Placa | Conductor | Cap. Peso | Cap. Volumen |
|---|---|---|---|
| ABC-123 | Juan Pérez | 500 kg | 10 m³ |
| DEF-456 | María López | 300 kg | 6 m³ |
| GHI-789 | Carlos Ruiz | 800 kg | 15 m³ |

---

## Endpoints Disponibles

### Shipments

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/shipments` | Crear un nuevo envío |
| `GET` | `/api/shipments/{id}` | Obtener envío por ID |
| `GET` | `/api/shipments/tracking/{code}` | Obtener por código de rastreo |
| `PATCH` | `/api/shipments/{id}/status` | Actualizar estado |
| `POST` | `/api/shipments/{id}/assign` | Asignar a vehículo |
| `GET` | `/api/shipments/overdue` | Envíos atrasados por SLA |

### Reports

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/reports/driver-performance` | Métricas de todos los conductores |
| `GET` | `/api/reports/driver-performance/{driverId}` | Métricas por conductor |

### Vehicles

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/vehicles` | Listar vehículos con carga actual |
| `GET` | `/api/vehicles/{id}` | Obtener vehículo por ID |

---

## Se adjunta una colección swagger para testeo en la raíz del repositorio
## Ejemplos de Uso (curl)

### Crear un Envío

```bash
curl -X POST https://localhost:5001/api/shipments \
  -H "Content-Type: application/json" \
  -d '{
    "senderName": "Ana García",
    "senderPhone": "3001234567",
    "senderAddress": "Calle 100 # 15-30, Bogotá",
    "recipientName": "Carlos Mendoza",
    "recipientPhone": "3219876543",
    "recipientAddress": "Carrera 50 # 80-45, Medellín",
    "packageWeight": 5.0,
    "packageDimensions": "30x20x15",
    "packageType": 2,
    "serviceType": 1,
    "originCityId": 1,
    "destinationCityId": 2
  }'
```

> `packageType`: 0=Document, 1=Package, 2=Fragile, 3=Perishable  
> `serviceType`: 0=Standard, 1=Express, 2=SameDay

**Respuesta esperada (201 Created):**
```json
{
  "id": 1,
  "trackingCode": "CM-00000001",
  "status": "Created",
  "serviceType": "Express",
  "packageType": "Fragile",
  "totalCost": 40950.00,
  ...
}
```

### Rastrear un Envío

```bash
curl https://localhost:5001/api/shipments/tracking/CM-00000001
```

### Asignar a Vehículo (se puede asignar manualmente enviando el Id o automáticamente enviando la petición sin el vehicleId)

```bash
curl -X POST https://localhost:5001/api/shipments/1/assign \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": 1,
    "assignedBy": "operaciones@couriermax.co"
  }'
```

### Cambiar Estado a En Tránsito

```bash
curl -X PATCH https://localhost:5001/api/shipments/1/status \
  -H "Content-Type: application/json" \
  -d '{
    "newStatus": 2,
    "changedBy": "conductor.juan@couriermax.co"
  }'
```

> `newStatus`: 0=Created, 1=Assigned, 2=InTransit, 3=Delivered, 4=Cancelled

### Cancelar un Envío

```bash
curl -X PATCH https://localhost:5001/api/shipments/1/status \
  -H "Content-Type: application/json" \
  -d '{
    "newStatus": 4,
    "changedBy": "admin@couriermax.co",
    "reason": "El cliente solicitó cancelación por cambio de planes"
  }'
```

### Consultar Envíos Atrasados

```bash
curl "https://localhost:5001/api/shipments/overdue?from=2026-01-01&to=2026-12-31"
```

### Reporte de Desempeño de Conductores

```bash
curl https://localhost:5001/api/reports/driver-performance
```

### Reporte por Conductor

```bash
curl https://localhost:5001/api/reports/driver-performance/1
```

### Listar Vehículos

```bash
curl https://localhost:5001/api/vehicles
```

---

## Reglas de Negocio Implementadas

| Regla | Descripción | Ubicación |
|---|---|---|
| **RN-01** | Capacidad de vehículo (peso y volumen) | `Vehicle.EnsureCanAccommodate()` |
| **RN-02** | Días hábiles con festivos Colombia 2026 | `BusinessDayCalculator` |
| **RN-03** | Cancelación | `Shipment.ValidateTransition()` |
| **RN-04** | Validaciones de Datos | `CreateShipmentValidator` |
| **RN-05** | Código de rastreo único CM-XXXXXXXX | `ShipmentService.CreateAsync()` |

### Cálculo de Tarifas (RF-04)

```
Costo Total = (Base + Recargo Peso + Tarifa Distancia) × (1 + Recargo Paquete)

Base:           Standard=$8,000 | Express=$15,000 | SameDay=$25,000
Recargo Peso:   $1,500 × (kg - 2)  [solo si peso > 2 kg]
Recargo Paquete: Frágil=+30% | Perecedero=+25% | Otros=0%


```

---

## Pruebas

### Ejecutar Pruebas Unitarias

```bash
dotnet test tests/CourierMax.UnitTests/ --verbosity normal
```

**Cobertura (59 tests):**
- `ShipmentEntityTests` — Máquina de estados, transiciones válidas e inválidas
- `TariffCalculatorServiceTests` — Todas las combinaciones de tarifa (incluye ejemplo del enunciado)
- `BusinessDayCalculatorTests` — Festivos colombianos, fines de semana, cálculo SLA
- `CreateShipmentValidatorTests` — Teléfono, peso, dimensiones, ciudades

### Ejecutar Pruebas de Integración

> ⚠️ Las pruebas de integración usan SQLite in-memory — **no requieren SQL Server**.

```bash
dotnet test tests/CourierMax.IntegrationTests/ --verbosity normal
```

### Ejecutar Todas las Pruebas

```bash
dotnet test CourierMax.sln
```

---

## Manejo de Errores

La API sigue **RFC 7807 Problem Details** para todas las respuestas de error:

```json
{
  "status": 422,
  "type": "https://couriermax.api/errors/vehicle_capacity_exceeded",
  "title": "Business Rule Violation",
  "detail": "Vehicle 'ABC-123' capacity exceeded: Weight 350 kg exceeds available capacity 280.50 kg",
  "errorCode": "VEHICLE_CAPACITY_EXCEEDED",
  "traceId": "0HN8..."
}
```

| Código HTTP | Cuándo se usa |
|---|---|
| 201 | Envío creado exitosamente |
| 200 | Operación exitosa |
| 400 | Validación fallida (FluentValidation) |
| 404 | Recurso no encontrado |
| 409 | Conflicto (código de rastreo duplicado) |
| 422 | Regla de negocio violada |
| 429 | Rate limit excedido (100 req/min) |
| 500 | Error interno del servidor |
