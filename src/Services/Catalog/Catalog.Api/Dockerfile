FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Catalog/Catalog.Api/Catalog.Api.csproj", "src/Services/Catalog/Catalog.Api/"]
COPY ["src/Services/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Services/Catalog/Catalog.Application/"]
COPY ["src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Services/Catalog/Catalog.Infrastructure/"]
COPY ["src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Services/Catalog/Catalog.Domain/"]
COPY ["src/BuildingBlocks/EventBus.Contracts/EventBus.Contracts.csproj", "src/BuildingBlocks/EventBus.Contracts/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
RUN dotnet restore "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Catalog/Catalog.Api"
RUN dotnet build "Catalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
