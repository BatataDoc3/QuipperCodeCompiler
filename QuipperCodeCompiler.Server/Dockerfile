# syntax=docker/dockerfile:1

# Use a Windows Server Core or Nano Server image for the SDK stage
# The 'windowsservercore-ltsc2022' or 'nanoserver-ltsc2022' tags are common.
# Note: Windows containers require the Docker daemon to be running in Windows mode.
FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build 
WORKDIR C:\App

COPY . .\

RUN dotnet restore

# Fix: Use Windows paths (backslashes) for the output path
RUN dotnet publish -o out

# Use the corresponding Windows Server Core or Nano Server for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore-ltsc2022
WORKDIR C:\App

# Fix: Use Windows paths for copying
COPY --from=build C:\App\out .

# Fix: Ensure required directories exist and have permissions (Windows handles this better, but we ensure existence)
RUN mkdir C:\App\CodeFiles

# Fix: Ensure the Windows image knows to use the correct DLL
ENTRYPOINT ["dotnet", "QuipperCodeCompiler.Server.dll"]