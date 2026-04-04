# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MapaClientes.Api.dll"]