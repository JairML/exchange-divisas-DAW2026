FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["X-Chang.API/X-Chang.API.csproj", "X-Chang.API/"]
COPY ["X-Chang.CORE/X-Chang.CORE.csproj", "X-Chang.CORE/"]
RUN dotnet restore "X-Chang.API/X-Chang.API.csproj"
COPY . .
RUN dotnet build "X-Chang.API/X-Chang.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "X-Chang.API/X-Chang.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "X-Chang.API.dll"]
