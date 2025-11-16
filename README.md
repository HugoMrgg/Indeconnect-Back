# IndeConnect -- Complete Backend Documentation

This repository contains the full backend for **IndeConnect**, a
platform designed to manage independent workers, missions, availability
periods, authentication flows, and administrative functions.\
It is built using **ASP.NET Core (.NET 9)**, **Entity Framework Core**,
and **PostgreSQL**, fully orchestrated through **Docker Compose**.

This README provides a complete guide to:

-   Understanding the project structure\
-   Configuring your development environment\
-   Building & running the backend via Docker\
-   API structure & conventions\
-   Database & migrations\
-   Authentication (JWT)\
-   Deployment notes\
-   Troubleshooting\
-   Contribution workflow

  ------------------------------------------------------------------------

## 1. Project Structure

The repository uses a **clean architecture-inspired** organization:

      IndeConnect-Back/
      ├── IndeConnect-Back.sln                   # Main solution
      ├── IndeConnect-Back/                      # (Optional bootstrap project)
      ├── IndeConnect-Back.Domain/               # Domain entities, enums, core logic
      │    ├── Entities/
      │    ├── Enums/
      │    └── Exceptions/
      ├── IndeConnect-Back.Application/          # Use cases, DTOs, validation, services
      │    ├── Interfaces/
      │    ├── Services/
      │    └── Validators/
      ├── IndeConnect-Back.Infrastructure/       # EF Core, database, repositories
      │    ├── Context/
      │    ├── Migrations/
      │    └── Repositories/
      └── IndeConnect-Back.Web/                  # API layer (controllers, middleware)
          ├── Controllers/
          ├── DTO/
          ├── Middleware/
          └── Program.cs

### Layers Summary

    -----------------------------------------------------------------------------
    Layer                Responsibility                     Contains
    -------------------- ---------------------------------- ---------------------
    **Domain**           Business rules                     Entities, enums

    **Application**      Use cases & logic                  Services, interfaces,
                                                            validation

    **Infrastructure**   External systems                   DbContext,
                                                            PostgreSQL,
                                                            repository
                                                            implementations

    **Web**              API layer                          Controllers,
                                                            middleware, routing
    -----------------------------------------------------------------------------

  ------------------------------------------------------------------------

## 2. Environment Requirements

Make sure you have the following installed:

### Required

-   **.NET 9.0 SDK**
-   **Docker Desktop** (or Docker Engine)
-   **Docker Compose v3.9+**

### Optional tools

-   **pgAdmin** or **TablePlus** for database inspection\
-   **Visual Studio / VS Code / JetBrains Rider**

  ------------------------------------------------------------------------

## 3. Environment Configuration (`.env`)

Copy the example file:

  ``` bash
  cp env.example .env
  ```

### `.env` contents explained:

      POSTGRES_DB=indeconnect
      POSTGRES_USER=indeconnect
      POSTGRES_PASSWORD=indeconnect
      ASPNETCORE_ENVIRONMENT=Development

      # Auto-built connection string
      CONNECTIONSTRINGS__DEFAULT=Host=db;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}

### How it is used

    Variable                       Used by    Purpose
    ------------------------------ ---------- ---------------------------------------
    POSTGRES_DB                    DB + API   Database name
    POSTGRES_USER                  DB + API   PostgreSQL username
    POSTGRES_PASSWORD              DB + API   PostgreSQL password
    ASPNETCORE_ENVIRONMENT         API        Sets Development, Production, Staging
    CONNECTIONSTRINGS\_\_DEFAULT   API        EF Core connection string

  ------------------------------------------------------------------------

## 4. Running the Backend With Docker

From the root of the repository:

### Build and run all services

  ``` bash
  docker compose up --build
  ```

### Run in background

  ``` bash
  docker compose up -d
  ```

### Stop everything

  ``` bash
  docker compose down
  ```

### Remove all volumes (DATABASE INCLUDED)

  ``` bash
  docker compose down -v
  ```

  ------------------------------------------------------------------------

## 5. Available Services in Docker

| Service     | Port | Description          |
|-------------|------|----------------------|
| **api**     | 8080 | ASP.NET Core Web API |
| **db**      | 5432 | PostgreSQL database  |
| **pgadmin** | 5050 | Database admin UI    |

  ------------------------------------------------------------------------

## 6. API Endpoints

### Base URL

    http://localhost:8080

### Swagger (OpenAPI)

      http://localhost:8080/swagger

### Healthcheck

      http://localhost:8080/health

### Example controllers

-   `AuthController`\
-   `IndependentController`\
-   `MissionController`\
-   `AvailabilityController`\
-   `AdminController`

All endpoints follow the structure:

```
/api/{controller}/{action}
```
  ------------------------------------------------------------------------

## 7. Authentication (JWT)

The backend uses **JWT Bearer tokens**.

### Token generation

Implemented in `AuthService`.

### Token validation

Configured in `Program.cs`:

-   Validates signature\
-   Validates expiration\
-   Validates issuer/audience (if configured)

### Adding authorization to endpoints

      [Authorize]

Or using custom policy:

      [RoleAuthorization(UserRole.Admin)]

  ------------------------------------------------------------------------

## 8. Database

### ORM: Entity Framework Core

-   Configured in `IndeConnect-Back.Infrastructure`
-   Uses PostgreSQL provider
-   Supports automatic migrations (optional)

### Apply migrations manually

  ``` bash
  dotnet ef database update \
    --project IndeConnect-Back.Infrastructure \
    --startup-project IndeConnect-Back.Web
  ```

### Generate new migration

  ``` bash
  dotnet ef migrations add MigrationName \
    --project IndeConnect-Back.Infrastructure \
    --startup-project IndeConnect-Back.Web
  ```

  ------------------------------------------------------------------------

## 9. Data Models (Overview)

### Users

-   Admin
-   Independent worker

### Missions

-   Title
-   Description
-   Attached independent
-   Start/end date

### Availability Periods

-   Start date
-   End date
-   Status

  ------------------------------------------------------------------------

## 10. Error Handling

All exceptions pass through `ExceptionMiddleware`.

Errors are returned as:

  ``` json
  {
    "status": 400,
    "error": "Invalid mission request",
    "details": "Description of the issue"
  }
  ```

  ------------------------------------------------------------------------

## 11. Dockerfile Summary

The Dockerfile:

-   Builds using `mcr.microsoft.com/dotnet/sdk:9.0`
-   Publishes the Web project
-   Runs from `mcr.microsoft.com/dotnet/aspnet:9.0`
-   Uses non-root user
-   Exposes port `8080`

  ------------------------------------------------------------------------

## 12. docker-compose Summary

Main services:

-   **api**\
-   **db (Postgres)**\
-   **pgadmin** (optional)

Healthchecks ensure DB is ready before API boots.

  ------------------------------------------------------------------------

## 13. Logging

-   Uses ASP.NET Core built-in logging
-   Logs written to console in Docker
-   Can be extended using Serilog (recommended)

  ------------------------------------------------------------------------

## 14. Deployment Notes

Recommended hosting environments:

-   Docker Swarm
-   Kubernetes
-   Azure Container Apps
-   AWS ECS

Use environment variables for:

-   JWT secret
-   Database credentials
-   SMTP credentials (if email added later)

  ------------------------------------------------------------------------

## 15. Recommended Folder Permissions

Ensure `docker-data/` is writable:

  ``` bash
  chmod -R 777 docker-data
  ```

  ------------------------------------------------------------------------

## 16. Troubleshooting

### API won't connect to PostgreSQL

-   Ensure `.env` matches `docker-compose.yml`
-   Ensure the API is using `Host=db;` not `localhost`

### Database not created

Run:

  ``` bash
  docker compose down -v
  docker compose up --build
  ```

### JWT errors

Ensure `JWT_SECRET` is set.

  ------------------------------------------------------------------------