# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY programacion-proyecto-backend.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose port (default, actual port is set via PORT environment variable at runtime)
EXPOSE 5148

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=5148

# The application will use the PORT environment variable set at runtime
# This EXPOSE is for documentation; the actual port mapping is done in docker-compose
ENTRYPOINT ["dotnet", "programacion-proyecto-backend.dll"]

