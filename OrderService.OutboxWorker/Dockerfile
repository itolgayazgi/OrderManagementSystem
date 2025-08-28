# Stage 1: Base image (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build image (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Önce csproj dosyasýný kopyala ve restore et
COPY ["OrderService.OutboxWorker/OrderService.OutboxWorker.csproj", "OrderService.OutboxWorker/"]
RUN dotnet restore "OrderService.OutboxWorker/OrderService.OutboxWorker.csproj"

# Sonra tüm source code'u kopyala
COPY . .

# Build ve publish
WORKDIR "/src/OrderService.OutboxWorker"
RUN dotnet build "OrderService.OutboxWorker.csproj" -c Release -o /app/build
RUN dotnet publish "OrderService.OutboxWorker.csproj" -c Release -o /app/publish

# Stage 3: Final image (base'den türetilir)
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.OutboxWorker.dll"]