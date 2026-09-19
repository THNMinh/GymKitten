# Stage 1: Base runtime image (.NET 8 ASP.NET Core)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Stage 2: Build image with .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for caching restore layers
COPY ["GymKitten.API/GymKitten.API.csproj", "GymKitten.API/"]
COPY ["GymKitten.Application/GymKitten.Application.csproj", "GymKitten.Application/"]
COPY ["GymKitten.Domain/GymKitten.Domain.csproj", "GymKitten.Domain/"]
COPY ["GymKitten.Infrastructure/GymKitten.Infrastructure.csproj", "GymKitten.Infrastructure/"]

# Restore packages
RUN dotnet restore "GymKitten.API/GymKitten.API.csproj"

# Copy all source code
COPY . .

# Build API project
WORKDIR "/src/GymKitten.API"
RUN dotnet build "GymKitten.API.csproj" -c Release -o /app/build

# Stage 3: Publish output
FROM build AS publish
RUN dotnet publish "GymKitten.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GymKitten.API.dll"]
