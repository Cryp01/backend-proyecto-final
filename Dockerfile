FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY programacion-proyecto-backend.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:3000

EXPOSE 3000

ENTRYPOINT ["dotnet", "programacion-proyecto-backend.dll"]
