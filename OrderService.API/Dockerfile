# -------------------
# 1. Build stage
# -------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projeyi kopyala
COPY ["OrderService.API/OrderService.API.csproj", "OrderService.API/"]
COPY ["OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]

# NuGet restore
RUN dotnet restore "OrderService.API/OrderService.API.csproj"

# Tüm source kodunu kopyala ve publish et
COPY . .
WORKDIR "/src/OrderService.API"
RUN dotnet publish "OrderService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------
# 2. Runtime stage
# -------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Published output'u kopyala
COPY --from=build /app/publish .

# Ortam değişkenleri ve port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Uygulamayı çalıştır
ENTRYPOINT ["dotnet", "OrderService.API.dll"]