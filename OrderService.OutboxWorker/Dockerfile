# ---- Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj'larý tek tek kopyala (restore cache için)
COPY ["OrderService.OutboxWorker/OrderService.OutboxWorker.csproj", "OrderService.OutboxWorker/"]
COPY ["OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]
	
RUN dotnet restore "OrderService.OutboxWorker/OrderService.OutboxWorker.csproj"

# kaynak kodu kopyala ve publish et
COPY . .
WORKDIR /src/OrderService.OutboxWorker
RUN dotnet publish -c Release -o /app/publish /p:PublishTrimmed=false /p:SelfContained=false /p:UseAppHost=false

# ---- Runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.OutboxWorker.dll"]