FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Lynx.sln .
COPY Directory.Build.props .
COPY src/Lynx.Abstractions/Lynx.Abstractions.csproj src/Lynx.Abstractions/
COPY src/ApiGateway/ApiGateway.csproj src/ApiGateway/

# Restore dependencies
RUN dotnet restore src/ApiGateway/ApiGateway.csproj

# Copy all source code
COPY src/Lynx.Abstractions/ src/Lynx.Abstractions/
COPY src/ApiGateway/ src/ApiGateway/

# Build and publish
RUN dotnet publish src/ApiGateway/ApiGateway.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
