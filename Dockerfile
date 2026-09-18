# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro para cachear o restore entre builds
COPY src/CozyCinema.Api/CozyCinema.Api.csproj src/CozyCinema.Api/
COPY src/CozyCinema.Application/CozyCinema.Application.csproj src/CozyCinema.Application/
COPY src/CozyCinema.Domain/CozyCinema.Domain.csproj src/CozyCinema.Domain/
COPY src/CozyCinema.Infrastructure/CozyCinema.Infrastructure.csproj src/CozyCinema.Infrastructure/

RUN dotnet restore src/CozyCinema.Api/CozyCinema.Api.csproj

# Agora copia o resto do código e publica
COPY src/ src/

RUN dotnet publish src/CozyCinema.Api/CozyCinema.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

EXPOSE 8080

# Render injeta PORT dinamicamente (padrão 10000); Fly.io não define PORT, então
# cai no fallback 8080 (o mesmo valor fixado no internal_port do fly.toml).
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet CozyCinema.Api.dll"]
