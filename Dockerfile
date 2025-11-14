# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY . .

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

RUN dotnet publish QuipperCodeCompiler.Server/QuipperCodeCompiler.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# 🚀 FORCE ASP.NET TO LISTEN ON PORT 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app .

ENTRYPOINT ["dotnet", "QuipperCodeCompiler.Server.dll"]
