# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo los .csproj primero para aprovechar la cache de capas de Docker
COPY X-Chang.API/*.csproj X-Chang.API/
COPY X-Chang.CORE/*.csproj X-Chang.CORE/
RUN dotnet restore X-Chang.API/X-Chang.API.csproj

# Copiar el resto del código y publicar
COPY . .
RUN dotnet publish X-Chang.API/X-Chang.API.csproj -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Railway inyecta PORT en tiempo de ejecucion; se usa forma shell para
# interpolar la variable al arrancar el contenedor (con fallback a 8080).
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet X-Chang.API.dll
