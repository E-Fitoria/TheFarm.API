# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TheFarm.API.csproj", "./"]
RUN dotnet restore "./TheFarm.API.csproj"

COPY . .
RUN dotnet publish "./TheFarm.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render inyecta PORT; ASP.NET debe escuchar en 0.0.0.0
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "TheFarm.API.dll"]