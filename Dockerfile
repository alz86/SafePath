# Use the official .NET SDK image to build and run the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy the .sln file from src/server/src directory
COPY ./src/server/src/*.sln .

# Copy the csproj files and restore any NuGet packages

# For server/src directory
COPY ./src/server/src/SafeMap.OSMParser/*.csproj ./src/server/src/SafeMap.OSMParser/
COPY ./src/server/src/SafePath.Application/*.csproj ./src/server/src/SafePath.Application/
COPY ./src/server/src/SafePath.Application.Contracts/*.csproj ./src/server/src/SafePath.Application.Contracts/
COPY ./src/server/src/SafePath.Blazor/*.csproj ./src/server/src/SafePath.Blazor/
COPY ./src/server/src/SafePath.DbMigrator/*.csproj ./src/server/src/SafePath.DbMigrator/
COPY ./src/server/src/SafePath.Domain/*.csproj ./src/server/src/SafePath.Domain/
COPY ./src/server/src/SafePath.Domain.Shared/*.csproj ./src/server/src/SafePath.Domain.Shared/
COPY ./src/server/src/SafePath.EntityFrameworkCore/*.csproj ./src/server/src/SafePath.EntityFrameworkCore/
COPY ./src/server/src/SafePath.HttpApi/*.csproj ./src/server/src/SafePath.HttpApi/
COPY ./src/server/src/SafePath.HttpApi.Client/*.csproj ./src/server/src/SafePath.HttpApi.Client/
COPY ./src/server/src/SafePath.HttpApi.Host/*.csproj ./src/server/src/SafePath.HttpApi.Host/

COPY ./src/server/src/SafePath.Application/Resources/Mappings.json ./src/server/src/SafePath.Application/Resources/
COPY ./src/server/common.props ./src/server/


# itinero 
COPY ./src/itinero/src/Itinero/*.csproj ./src/itinero/src/Itinero/
COPY ./src/itinero/src/Itinero.IO.Osm/*.csproj ./src/itinero/src/Itinero.IO.Osm/
COPY ./src/itinero/src/Itinero.Geo/*.csproj ./src/itinero/src/Itinero.Geo/
COPY ./src/itinero/src/Itinero.IO.Shape/*.csproj ./src/itinero/src/Itinero.IO.Shape/

COPY ./src/itinero/src/Itinero/Osm/Vehicles/*.lua ./src/itinero/src/Itinero/Osm/Vehicles/
COPY ./src/itinero/Itinero.Common.props ./src/itinero/

# Restore packages for SafePath.HttpApi.Host
RUN dotnet restore ./src/server/src/SafePath.HttpApi.Host/SafePath.HttpApi.Host.csproj

# For bind mounts during development, no COPY for source files as they'll be mounted
# Entry point defined to watch and run the SafePath.HttpApi.Host
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/server/src/SafePath.HttpApi.Host/SafePath.HttpApi.Host.csproj", "--no-restore"]
