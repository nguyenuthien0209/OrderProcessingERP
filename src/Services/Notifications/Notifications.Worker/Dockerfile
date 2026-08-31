FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Notifications/Notifications.Worker/Notifications.Worker.csproj", "src/Services/Notifications/Notifications.Worker/"]
COPY ["src/BuildingBlocks/EventBus.Contracts/EventBus.Contracts.csproj", "src/BuildingBlocks/EventBus.Contracts/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
RUN dotnet restore "src/Services/Notifications/Notifications.Worker/Notifications.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Notifications/Notifications.Worker"
RUN dotnet build "Notifications.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notifications.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Worker.dll"]
