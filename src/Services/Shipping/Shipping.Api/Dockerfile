FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Shipping/Shipping.Api/Shipping.Api.csproj", "src/Services/Shipping/Shipping.Api/"]
COPY ["src/Services/Shipping/Shipping.Application/Shipping.Application.csproj", "src/Services/Shipping/Shipping.Application/"]
COPY ["src/Services/Shipping/Shipping.Infrastructure/Shipping.Infrastructure.csproj", "src/Services/Shipping/Shipping.Infrastructure/"]
COPY ["src/Services/Shipping/Shipping.Domain/Shipping.Domain.csproj", "src/Services/Shipping/Shipping.Domain/"]
COPY ["src/BuildingBlocks/EventBus.Contracts/EventBus.Contracts.csproj", "src/BuildingBlocks/EventBus.Contracts/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
RUN dotnet restore "src/Services/Shipping/Shipping.Api/Shipping.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Shipping/Shipping.Api"
RUN dotnet build "Shipping.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shipping.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shipping.Api.dll"]
