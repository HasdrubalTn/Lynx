FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files for dependency resolution
COPY *.sln .
COPY Directory.Build.props .
COPY src/Lynx.Abstractions/Lynx.Abstractions.csproj src/Lynx.Abstractions/
COPY src/NotificationService/NotificationService.csproj src/NotificationService/

# Restore dependencies
RUN dotnet restore src/NotificationService/NotificationService.csproj

# Copy all source code
COPY src/Lynx.Abstractions/ src/Lynx.Abstractions/
COPY src/NotificationService/ src/NotificationService/

# Build and publish
RUN dotnet publish src/NotificationService/NotificationService.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
