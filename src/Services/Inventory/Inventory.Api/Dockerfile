FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Inventory/Inventory.Api/Inventory.Api.csproj", "src/Services/Inventory/Inventory.Api/"]
COPY ["src/Services/Inventory/Inventory.Application/Inventory.Application.csproj", "src/Services/Inventory/Inventory.Application/"]
COPY ["src/Services/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "src/Services/Inventory/Inventory.Infrastructure/"]
COPY ["src/Services/Inventory/Inventory.Domain/Inventory.Domain.csproj", "src/Services/Inventory/Inventory.Domain/"]
COPY ["src/BuildingBlocks/EventBus.Contracts/EventBus.Contracts.csproj", "src/BuildingBlocks/EventBus.Contracts/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
RUN dotnet restore "src/Services/Inventory/Inventory.Api/Inventory.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Inventory/Inventory.Api"
RUN dotnet build "Inventory.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Inventory.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
