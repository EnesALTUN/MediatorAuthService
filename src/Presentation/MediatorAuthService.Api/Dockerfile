FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY src/Domain/MediatorAuthService.Domain.Core/*.csproj ./Domain/MediatorAuthService.Domain.Core/
COPY src/Domain/MediatorAuthService.Domain/*.csproj ./Domain/MediatorAuthService.Domain/
COPY src/Infrastructure/MediatorAuthService.Infrastructure/*.csproj ./Infrastructure/MediatorAuthService.Infrastructure/
COPY src/Application/MediatorAuthService.Application/*.csproj ./Application/MediatorAuthService.Application/
COPY src/Presentation/MediatorAuthService.Api/*.csproj ./Presentation/MediatorAuthService.Api/

COPY ./*.sln .

RUN dotnet restore Presentation/MediatorAuthService.Api/*.csproj
COPY . .
RUN dotnet publish src/Presentation/MediatorAuthService.Api/*.csproj -o /publish/

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "MediatorAuthService.Api.dll"]