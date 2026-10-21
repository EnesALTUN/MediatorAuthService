# MediatorAuthService
This project is an authentication and authorization microservice based on .NET 10.

## Features
- JWT access tokens with rotating refresh tokens (reuse detection, logout, session revocation)
- Role and permission based authorization, manageable at runtime through the API
- User management with soft delete
- Account lockout, rate limiting on credential endpoints and a strong password policy
- Secure password hashing (PBKDF2-SHA512)
- Pagination, filtering and sorting
- Centralized log management with Elasticsearch & Kibana, sensitive data redacted from logs
- Health checks and optional OpenTelemetry export
- Unit and integration tests, GitHub Actions CI

## Requirements
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server
- [Docker](https://www.docker.com/) (for Elasticsearch, Kibana, and SQL Server containers)

## Installation
1. Clone the repository:
    ```bash
    git clone https://github.com/EnesALTUN/MediatorAuthService.git
    cd MediatorAuthService
    ```
2. Restore dependencies:
    ```bash
    dotnet restore
    ```

### Option A: Running with Docker (Recommended)

3. Configure environment variables for Docker:
   - Copy `.env.example` to `.env` and fill in the required values:
    ```bash
    cp .env.example .env
    ```
   - `JWT_SECURITY_KEY` must be at least 32 characters; the application refuses to start otherwise.
   - Set `SEED_ADMIN_EMAIL` and `SEED_ADMIN_PASSWORD` to create the first SuperAdmin (see [Authorization](#authorization)).
   - Never commit the `.env` file to version control.

4. Create the external networks once (they are shared with other services):
    ```bash
    docker network create net_backendservices
    docker network create sql_db
    ```

5. Start all services with Docker:
    ```bash
    docker compose up -d
    ```
   The API listens on `http://localhost:5216`. The container runs as a non-root user and serves plain HTTP; terminate TLS at a reverse proxy.

### Option B: Running without Docker

3. Initialize User Secrets (run this once if you are using it for the first time to securely store sensitive data in the development environment):
    ```bash
    dotnet user-secrets init --project src/Presentation/MediatorAuthService.Api
    ```
4. Set the required secrets:
    ```bash
    dotnet user-secrets set "ConnectionStrings:Default" "<CONNECTION_STRING>" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "JwtTokenOption:SecurityKey" "<SECURITY_KEY_MIN_32_CHARS>" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "ElasticSearch:Uri" "http://localhost:9200" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "MediatR:LicenseKey" "<LICENSE_KEY>" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "Seed:AdminEmail" "<ADMIN_EMAIL>" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "Seed:AdminPassword" "<ADMIN_PASSWORD>" --project src/Presentation/MediatorAuthService.Api
    ```
   > For more information, see [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

5. Run the application:
    ```bash
    dotnet run --project src/Presentation/MediatorAuthService.Api
    ```

> **Note:** On startup, pending migrations are applied automatically (with retries while the database server starts) and the first SuperAdmin is seeded. Set `Database:InitializationMode` to `None` to manage the schema yourself.

## Authentication
| Endpoint | Description |
|---|---|
| `POST /api/v1/Auth/login` | Returns an access token (15 min) and a refresh token (30 days). |
| `POST /api/v1/Auth/refresh-token` | Exchanges a refresh token for a new pair. No access token is needed. |
| `POST /api/v1/Auth/logout` | Revokes a refresh token. |
| `GET /api/v1/Auth/me` | The signed-in user with roles and effective permissions. |

- Send the access token as `Authorization: Bearer <accessToken>`.
- Refresh tokens are single-use. Presenting a used token again revokes every session of the user.
- Changing the password, deactivating or deleting a user ends all of their sessions immediately, including issued access tokens.
- After 5 failed login attempts the account is locked for 15 minutes (`Lockout` section). Credential endpoints are rate limited per IP (`RateLimiting:Auth`); behind a reverse proxy, configure `ForwardedHeaders`. A locked account gets the same error as a wrong password.

## Authorization
Endpoints are protected by **permissions** (e.g. `Users.Read`, `Roles.Update`). Permissions are defined in code
(`Application/Common/Authorization/Permissions.cs`); **roles** group permissions and are stored in the database, so they can be managed from an admin screen without deployments.
Permission changes apply on the next request; users do not need to sign in again.

| Endpoint | Permission |
|---|---|
| `GET /api/v1/Permission` | `Roles.Read` – permission catalog for building role screens |
| `GET /api/v1/Role`, `GET /api/v1/Role/{id}` | `Roles.Read` |
| `POST /api/v1/Role` | `Roles.Create` |
| `PUT /api/v1/Role/{id}` | `Roles.Update` (name, description and the full permission list) |
| `DELETE /api/v1/Role/{id}` | `Roles.Delete` |
| `GET /api/v1/User`, `GET /api/v1/User/{email}` | `Users.Read` |
| `GET /api/v1/User/{id}`, `GET /api/v1/User/{id}/roles` | Own record, or `Users.Read` |
| `POST /api/v1/User` | `Users.Create` |
| `PUT /api/v1/User/{id}` | Own record, or `Users.Update` (also required to activate/deactivate) |
| `DELETE /api/v1/User/{id}` | `Users.Delete` |
| `PUT /api/v1/User/{id}/roles` | `Users.AssignRoles` |
| `POST /api/v1/User/change-password` | Signed-in user |

Rules that prevent privilege escalation:
- **SuperAdmin** is a system role that holds every permission. It cannot be edited or deleted, only a SuperAdmin can assign or remove it, and the last SuperAdmin cannot lose it.
- You can only add or remove permissions you hold yourself, whether on a role or by assigning roles to a user.
- You can only update, delete or change the roles of users whose permissions are a subset of yours. Users cannot delete or deactivate themselves.

The first SuperAdmin is created from `Seed:AdminEmail` / `Seed:AdminPassword` while no SuperAdmin exists. If a user with that e-mail already exists, the role is assigned without changing the password.
No credentials are hard-coded.

> **Upgrading from an earlier version:** earlier versions seeded `admin@gmail.com` with a publicly known password. If that account still uses it, it is disabled at startup. It can be promoted with `Seed:AdminEmail` only when `Seed:AdminPassword` is also set, which replaces the old password.

## Configuration
- All configurations are managed via the `appsettings.json` file or environment variables.
- Do not store sensitive information such as connection strings directly in `appsettings.json`.
- For Docker-based deployments, use the `.env` file (see `.env.example`).

| Key | Description |
|---|---|
| `JwtTokenOption` | Issuer, audiences, signing key (≥ 32 bytes) and token lifetimes. Validated at startup. |
| `Cors:AllowedOrigins` | Origins allowed to call the API (e.g. the admin UI). Empty means none. |
| `Lockout`, `RateLimiting:Auth` | Brute-force protection. |
| `Seed` | First SuperAdmin. |
| `Database:InitializationMode` | `Migrate` (default), `EnsureCreated` or `None`. |
| `HttpsRedirection:Enabled` | Disable when TLS is terminated by a proxy. |
| `ForwardedHeaders:KnownProxies`, `ForwardedHeaders:KnownNetworks` | IP addresses / CIDR ranges of the reverse proxy. `X-Forwarded-For` is trusted only from these, so rate limiting and logs see the real client IP. Empty means the headers are ignored. |
| `ElasticSearch` | Log sink. Leave `Uri` empty to disable; `Username`/`Password` for secured clusters. |
| `OpenTelemetry:OtlpEndpoint` | Exports traces and metrics over OTLP when set. |

## Logging
This project uses **Serilog** with **Elasticsearch** as the log sink. All application logs are shipped to Elasticsearch and can be visualized via **Kibana**.
Passwords and tokens are redacted and e-mail addresses are masked in request logs. Error responses contain a `traceId` that matches the logs.

- Kibana UI: `http://localhost:5601`
- Elasticsearch: `http://localhost:9200`

Log levels are configured per namespace in `appsettings.json`:
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning"
    }
  }
}
```

> **Note:** Elasticsearch requires `vm.max_map_count` to be at least `262144`. On Windows with WSL2, run the following command as administrator:
> ```powershell
> wsl -d docker-desktop sysctl -w vm.max_map_count=262144
> ```

## Health Checks
- `GET /health/live` – the process is running.
- `GET /health/ready` – the database is reachable.

## Tests
```bash
dotnet test
```
Integration tests host the real application against an in-memory SQLite database, so no external services are needed.

## Project Structure
- `src/Presentation/MediatorAuthService.Api` : API project (controllers, authentication, authorization policies, middleware)
- `src/Application` : Use cases (CQRS handlers), validation, abstractions
- `src/Domain` : Domain models and rules
- `src/Infrastructure` : Data access (EF Core, UnitOfWork), token issuing, permission cache
- `tests` : Unit and integration tests

## Technologies Used
- **.NET 10** – Application infrastructure and API development
- **ASP.NET Core** – Web API, authorization policies, rate limiting, health checks
- **Entity Framework Core** – ORM and database operations
- **JWT (JSON Web Token)** – Authentication
- **User Secrets** – Secure secret management in development
- **FluentValidation** – Model validation
- **Swagger / Swashbuckle** – API documentation
- **MediatR** – CQRS and mediator pattern
- **API Versioning (Asp.Versioning.Mvc)** – API version management
- **Generic Repository & UnitOfWork** – Data access and transaction management
- **Docker** – Containerization and deployment
- **Serilog** – Structured logging
- **Elasticsearch** – Centralized log storage and search
- **Kibana** – Log visualization and monitoring dashboard
- **OpenTelemetry** – Traces and metrics
- **xUnit** – Unit and integration tests

## API Versioning
This project supports API versioning using the [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) package.  
By default, versioning is done via URL segment:  
`/api/v1/Auth`

To add a new version, you can add an attribute like `[ApiVersion(2.0)]` to your controller.

## License
This project is licensed under the MIT License.
