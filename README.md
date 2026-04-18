# MediatorAuthService
This project is an authentication microservice based on .NET 9.

## Features
- JWT-based authentication
- User management
- Advanced configuration options
- Modern .NET 9 architecture
- Pagination support
- Secure password management with hashing
- Multiple DbContext and UnitOfWork infrastructure
- Centralized log management with Elasticsearch & Kibana

## Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (or a compatible database)
- [Docker](https://www.docker.com/) (for Elasticsearch, Kibana, and SQL Server containers)

## Installation
1. Clone the repository:
2. Restore dependencies:

### Option A: Running with Docker (Recommended)

3. Configure environment variables for Docker:
   - Copy `.env.example` to `.env` and fill in the required values:
    ```bash
    cp .env.example .env
    ```
   - Never commit the `.env` file to version control.

4. Start all services with Docker:
    ```bash
    docker compose up -d
    ```

### Option B: Running without Docker

3. Initialize User Secrets (run this once if you are using it for the first time to securely store sensitive data in the development environment):
    ```bash
    dotnet user-secrets init --project src/Presentation/MediatorAuthService.Api
    ```
4. Set the required secrets:
    ```bash
    dotnet user-secrets set "ConnectionStrings:Default" "<CONNECTION_STRING>" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "JwtTokenOption:SecurityKey" "<SECURITY_KEY>" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "ElasticSearch:Uri" "http://localhost:9200" --project src/Presentation/MediatorAuthService.Api
    dotnet user-secrets set "MediatR:LicenseKey" "<LICENSE_KEY>" --project src/Presentation/MediatorAuthService.Api
    ```
   > For more information, see [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

5. Run the application:

> **Note:** When the application starts for the first time, the database and required tables will be created automatically thanks to the `app.ApplyMigration();` operation. There is no need for an extra migration or manual database creation step.

## Configuration
- All configurations are managed via the `appsettings.json` file or environment variables.
- Do not store sensitive information such as connection strings directly in `appsettings.json`.
- For Docker-based deployments, use the `.env` file (see `.env.example`).

## Logging
This project uses **Serilog** with **Elasticsearch** as the log sink. All application logs are shipped to Elasticsearch and can be visualized via **Kibana**.

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

## Project Structure
- `src/Presentation/MediatorAuthService.Api` : API project
- `src/Application` : Application layer
- `src/Domain` : Domain models and rules
- `src/Infrastructure` : Data access and infrastructure layer

## Technologies Used
- **.NET 9** – Application infrastructure and API development
- **ASP.NET Core** – Web API and middleware architecture
- **Entity Framework Core** – ORM and database operations
- **JWT (JSON Web Token)** – Authentication and authorization
- **User Secrets** – Secure secret management in development
- **Dependency Injection** – Dependency management
- **Automapper** – Object mapping
- **FluentValidation** – Model validation
- **Swagger / Swashbuckle** – API documentation
- **MediatR** – CQRS and mediator pattern
- **API Versioning (Microsoft.AspNetCore.Mvc.Versioning)** – API version management
- **Pagination** – Efficient data listing for large datasets
- **Hashing** – Secure password storage and verification
- **Multiple DbContext & UnitOfWork** – Integrated transaction management with multiple DbContexts
- **Docker** – Containerization and deployment
- **Serilog** – Structured logging
- **Elasticsearch** – Centralized log storage and search
- **Kibana** – Log visualization and monitoring dashboard

## API Versioning
This project supports API versioning using the [Microsoft.AspNetCore.Mvc.Versioning](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/versioning) package.  
By default, versioning is done via URL segment:  
`/api/v1/Auth`

To add a new version, you can add an attribute like `[ApiVersion("2.0")]` to your controller.

## License
This project is licensed under the MIT License.
