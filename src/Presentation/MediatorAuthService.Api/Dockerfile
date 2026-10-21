FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first so that the restore layer is cached until dependencies change.
COPY src/Domain/MediatorAuthService.Domain.Core/*.csproj src/Domain/MediatorAuthService.Domain.Core/
COPY src/Domain/MediatorAuthService.Domain/*.csproj src/Domain/MediatorAuthService.Domain/
COPY src/Application/MediatorAuthService.Application/*.csproj src/Application/MediatorAuthService.Application/
COPY src/Infrastructure/MediatorAuthService.Infrastructure/*.csproj src/Infrastructure/MediatorAuthService.Infrastructure/
COPY src/Presentation/MediatorAuthService.Api/*.csproj src/Presentation/MediatorAuthService.Api/

RUN dotnet restore src/Presentation/MediatorAuthService.Api/MediatorAuthService.Api.csproj

COPY src/ src/
RUN dotnet publish src/Presentation/MediatorAuthService.Api/MediatorAuthService.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Non-root user provided by the official .NET images.
USER $APP_UID

ENTRYPOINT ["dotnet", "MediatorAuthService.Api.dll"]
