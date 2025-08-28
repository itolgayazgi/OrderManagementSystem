# ----- build -----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# solution ve projeleri kopyala
COPY ./OrderService.API/OrderService.API.csproj ./OrderService.API/
COPY ./OrderService.Application/OrderService.Application.csproj ./OrderService.Application/
COPY ./OrderService.Infrastructure/OrderService.Infrastructure.csproj ./OrderService.Infrastructure/
COPY ./OrderService.Domain/OrderService.Domain.csproj ./OrderService.Domain/
RUN dotnet restore ./OrderService.API/OrderService.API.csproj

# tüm kodu kopyala ve publish et
COPY . .
RUN dotnet publish ./OrderService.API/OrderService.API.csproj -c Release -o /app/publish

# ----- runtime -----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Container içinde HTTPS yerine HTTP kullanacaðýz
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.API.dll"]