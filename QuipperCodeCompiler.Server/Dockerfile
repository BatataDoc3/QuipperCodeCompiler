# syntax=docker/dockerfile:1

# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# OPTIMIZATION: Copy only the .csproj file found in the CURRENT directory
COPY *.csproj ./
# Restore dependencies for this project only
RUN dotnet restore

# Copy the rest of the source code from the CURRENT directory
COPY . .

# Build and Publish (Release configuration)
RUN dotnet publish -c Release -o /out

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Default port for .NET 8 is 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "QuipperCodeCompiler.Server.dll"]