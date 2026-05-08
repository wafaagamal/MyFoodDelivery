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
COPY ["services/MyFoodDelivery.DeliverySvc/src/Presentation/DeliverySvc.HttpApi.Host/DeliverySvc.HttpApi.Host.csproj", "services/MyFoodDelivery.DeliverySvc/src/Presentation/DeliverySvc.HttpApi.Host/"]
COPY ["services/MyFoodDelivery.DeliverySvc/src/Presentation/DeliverySvc.HttpApi/DeliverySvc.HttpApi.csproj", "services/MyFoodDelivery.DeliverySvc/src/Presentation/DeliverySvc.HttpApi/"]
COPY ["services/MyFoodDelivery.DeliverySvc/src/Application/DeliverySvc.Application/DeliverySvc.Application.csproj", "services/MyFoodDelivery.DeliverySvc/src/Application/DeliverySvc.Application/"]
COPY ["services/MyFoodDelivery.DeliverySvc/src/Application/DeliverySvc.Application.Contracts/DeliverySvc.Application.Contracts.csproj", "services/MyFoodDelivery.DeliverySvc/src/Application/DeliverySvc.Application.Contracts/"]
COPY ["services/MyFoodDelivery.DeliverySvc/src/Domain/DeliverySvc.Domain/DeliverySvc.Domain.csproj", "services/MyFoodDelivery.DeliverySvc/src/Domain/DeliverySvc.Domain/"]
COPY ["services/MyFoodDelivery.Shared/MyFoodDelivery.Shared.csproj", "services/MyFoodDelivery.Shared/"]
COPY ["services/MyFoodDelivery.DeliverySvc/src/Infrastructure/DeliverySvc.Infrastructure/DeliverySvc.Infrastructure.csproj", "services/MyFoodDelivery.DeliverySvc/src/Infrastructure/DeliverySvc.Infrastructure/"]
RUN dotnet restore "./services/MyFoodDelivery.DeliverySvc/src/Presentation/DeliverySvc.HttpApi.Host/DeliverySvc.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/MyFoodDelivery.DeliverySvc/src/Presentation/DeliverySvc.HttpApi.Host"
RUN dotnet build "./DeliverySvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DeliverySvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeliverySvc.HttpApi.Host.dll"]