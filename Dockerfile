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

# Expose port (will be set by PORT environment variable)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# ASP.NET Core will use PORT environment variable automatically via ASPNETCORE_URLS
# If PORT is not set, it defaults to 8080
ENTRYPOINT ["dotnet", "programacion-proyecto-backend.dll"]

