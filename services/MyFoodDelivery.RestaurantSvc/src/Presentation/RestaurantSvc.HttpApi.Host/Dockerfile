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
COPY ["services/MyFoodDelivery.RestaurantSvc/src/Presentation/RestaurantSvc.HttpApi.Host/RestaurantSvc.HttpApi.Host.csproj", "services/MyFoodDelivery.RestaurantSvc/src/Presentation/RestaurantSvc.HttpApi.Host/"]
COPY ["services/MyFoodDelivery.RestaurantSvc/src/Presentation/RestaurantSvc.HttpApi/RestaurantSvc.HttpApi.csproj", "services/MyFoodDelivery.RestaurantSvc/src/Presentation/RestaurantSvc.HttpApi/"]
COPY ["services/MyFoodDelivery.RestaurantSvc/src/Application/RestaurantSvc.Application/RestaurantSvc.Application.csproj", "services/MyFoodDelivery.RestaurantSvc/src/Application/RestaurantSvc.Application/"]
COPY ["services/MyFoodDelivery.RestaurantSvc/src/Application/RestaurantSvc.Application.Contracts/RestaurantSvc.Application.Contracts.csproj", "services/MyFoodDelivery.RestaurantSvc/src/Application/RestaurantSvc.Application.Contracts/"]
COPY ["services/MyFoodDelivery.RestaurantSvc/src/Domain/RestaurantSvc.Domain/RestaurantSvc.Domain.csproj", "services/MyFoodDelivery.RestaurantSvc/src/Domain/RestaurantSvc.Domain/"]
COPY ["services/MyFoodDelivery.Shared/MyFoodDelivery.Shared.csproj", "services/MyFoodDelivery.Shared/"]
RUN dotnet restore "./services/MyFoodDelivery.RestaurantSvc/src/Presentation/RestaurantSvc.HttpApi.Host/RestaurantSvc.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/MyFoodDelivery.RestaurantSvc/src/Presentation/RestaurantSvc.HttpApi.Host"
RUN dotnet build "./RestaurantSvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RestaurantSvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantSvc.HttpApi.Host.dll"]