# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OrderService.API/OrderService.API.csproj","src/OrderService.API/"]
COPY ["src/OrderService.Infrastructure/OrderService.Infrastructure.csproj","src/OrderService.Infrastructure/"]
COPY ["src/OrderService.Domain/OrderService.Domain.csproj","src/OrderService.Domain/"]
COPY ["src/OrderService.Application/OrderService.Application.csproj","src/OrderService.Application/"]
RUN dotnet restore "src/OrderService.API/OrderService.API.csproj"
COPY src/ ./src/
WORKDIR /src/OrderService.API
RUN dotnet publish -c Release -o /app

# RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","OrderService.API.dll"]