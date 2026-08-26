
# FamilyPlanner

FamilyPlanner is a full-stack web application for family organization and collaboration. The application currently provides a shared shopping catalog, shopping lists, authentication, and real-time notifications.

The project is built as a small microservices-based system using ASP.NET Core, Next.js, PostgreSQL, RabbitMQ, SignalR, and Azure AD B2C.

## Features

* Manage categories and items in a shared family catalog.
* Create and manage shopping lists.
* Synchronize catalog and shopping-list changes between services through RabbitMQ.
* Receive real-time updates through SignalR.
* Authenticate users with Azure AD B2C.
* Isolate catalog data by family.
* Search catalog items using PostgreSQL full-text and similarity search.

## Architecture

```text
Next.js Web App
       |
       v
 GatewayService
       |
       +--> CatalogService --------> PostgreSQL
       |
       +--> ShoppingListService ---> PostgreSQL
                     |
                  RabbitMQ
                     |
                     v
             NotificationService
                     |
                   SignalR
                     |
                     v
                Web Client
```

### Backend

* ASP.NET Core 8
* Entity Framework Core
* PostgreSQL
* RabbitMQ
* MassTransit
* SignalR
* Microsoft Graph
* Azure Application Insights

Backend services:

* `CatalogService`
* `ShoppingListService`
* `NotificationService`
* `GatewayService`
* `Contracts`

### Frontend

* Next.js 16
* React 19
* TypeScript
* Redux Toolkit
* Flowbite React
* Tailwind CSS
* MSAL
* SignalR client

### Authentication

Authentication and authorization are provided through Azure AD B2C.

Catalog and shopping-list data are scoped to the authenticated user's family.

## Development Environment

Development uses a hybrid local environment.

PostgreSQL and RabbitMQ run in Docker, while the .NET services and Next.js frontend run directly on the development machine. This makes application development and debugging faster while keeping infrastructure dependencies reproducible.

### Start development infrastructure

From the repository root:

```powershell
docker compose --env-file .env.dev -f docker-compose.dev.yml up -d
```

Check container status:

```powershell
docker compose --env-file .env.dev -f docker-compose.dev.yml ps
```

The development infrastructure exposes:

| Service                  |  Port |
| ------------------------ | ----: |
| Catalog PostgreSQL       |  5432 |
| Shopping List PostgreSQL |  5433 |
| RabbitMQ                 |  5672 |
| RabbitMQ Management UI   | 15672 |

Stop the infrastructure:

```powershell
docker compose --env-file .env.dev -f docker-compose.dev.yml down
```

## Running Backend Services

Run backend services from separate terminals as needed:

```powershell
dotnet run --project src/CatalogService
dotnet run --project src/ShoppingListService
dotnet run --project src/NotificationService
dotnet run --project src/GatewayService
```

## Running the Frontend

```powershell
cd frontend/web-app
npm ci
npm run dev
```

Useful frontend verification commands:

```powershell
npm run lint
npm run build
```

## Build and Test

Build the complete backend solution:

```powershell
dotnet build FamilyPlanner.sln
```

Run all tests:

```powershell
dotnet test FamilyPlanner.sln
```

Check NuGet packages for known vulnerabilities:

```powershell
dotnet list FamilyPlanner.sln package --vulnerable
```

## Testing

The project contains unit and integration tests for the backend services.

Catalog integration tests use:

* `WebApplicationFactory`
* PostgreSQL Testcontainers
* MassTransit test harness
* Fake JWT authentication

Recent CatalogService improvements include regression coverage for:

* database persistence before integration-event publication;
* category and item mutation failure behavior;
* family isolation for catalog CRUD operations;
* family isolation in catalog search;
* exclusion of soft-deleted items from search results.

## Containerized Staging Environment

The staging configuration runs the complete application in Docker:

```powershell
docker compose --env-file .env.stage -f docker-compose.stage.yml up -d --build
```

Check the stack:

```powershell
docker compose --env-file .env.stage -f docker-compose.stage.yml ps
```

Stop it with:

```powershell
docker compose --env-file .env.stage -f docker-compose.stage.yml down
```

The staging environment contains the frontend, all backend services, PostgreSQL databases, and RabbitMQ.

## Current Development Status

The project recently completed a dependency and infrastructure refresh, including:

* removal of the obsolete Blazor frontend;
* modernization of the Next.js/React frontend;
* refreshed Dockerfiles and Docker Compose configuration;
* PostgreSQL 16 development and staging containers;
* refreshed ASP.NET Core dependencies;
* improved CatalogService consistency and family-data isolation;
* expanded unit and integration testing.

Application-level modernization is ongoing, beginning with CatalogService and ShoppingListService.

## Planned Features

Future FamilyPlanner functionality may include:

* meal planning;
* family event scheduling;
* recurring shopping items;
* improved household collaboration;
* family budgeting and expense tracking.

## Repository Structure

```text
FamilyPlanner/
├── frontend/
│   └── web-app/
├── src/
│   ├── CatalogService/
│   ├── Contracts/
│   ├── GatewayService/
│   ├── NotificationService/
│   └── ShoppingListService/
├── tests/
├── docker-compose.dev.yml
├── docker-compose.stage.yml
└── FamilyPlanner.sln
```
