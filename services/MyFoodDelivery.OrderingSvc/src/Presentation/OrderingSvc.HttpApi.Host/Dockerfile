# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/MyFoodDelivery.OrderingSvc/src/Presentation/OrderingSvc.HttpApi.Host/OrderingSvc.HttpApi.Host.csproj", "services/MyFoodDelivery.OrderingSvc/src/Presentation/OrderingSvc.HttpApi.Host/"]
COPY ["services/MyFoodDelivery.OrderingSvc/src/Presentation/OrderingSvc.HttpApi/OrderingSvc.HttpApi.csproj", "services/MyFoodDelivery.OrderingSvc/src/Presentation/OrderingSvc.HttpApi/"]
COPY ["services/MyFoodDelivery.OrderingSvc/src/Application/OrderingSvc.Application/OrderingSvc.Application.csproj", "services/MyFoodDelivery.OrderingSvc/src/Application/OrderingSvc.Application/"]
COPY ["services/MyFoodDelivery.OrderingSvc/src/Application/OrderingSvc.Application.Contracts/OrderingSvc.Application.Contracts.csproj", "services/MyFoodDelivery.OrderingSvc/src/Application/OrderingSvc.Application.Contracts/"]
COPY ["services/MyFoodDelivery.OrderingSvc/src/Domain/OrderingSvc.Domain/OrderingSvc.Domain.csproj", "services/MyFoodDelivery.OrderingSvc/src/Domain/OrderingSvc.Domain/"]
COPY ["services/MyFoodDelivery.Shared/MyFoodDelivery.Shared.csproj", "services/MyFoodDelivery.Shared/"]
COPY ["services/MyFoodDelivery.OrderingSvc/src/Infrastructure/OrderingSvc.Infrastructure/OrderingSvc.Infrastructure.csproj", "services/MyFoodDelivery.OrderingSvc/src/Infrastructure/OrderingSvc.Infrastructure/"]
RUN dotnet restore "./services/MyFoodDelivery.OrderingSvc/src/Presentation/OrderingSvc.HttpApi.Host/OrderingSvc.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/MyFoodDelivery.OrderingSvc/src/Presentation/OrderingSvc.HttpApi.Host"
RUN dotnet build "./OrderingSvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrderingSvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderingSvc.HttpApi.Host.dll"]