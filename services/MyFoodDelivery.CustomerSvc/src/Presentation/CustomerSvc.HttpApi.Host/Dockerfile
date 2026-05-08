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
COPY ["services/MyFoodDelivery.CustomerSvc/src/Presentation/CustomerSvc.HttpApi.Host/CustomerSvc.HttpApi.Host.csproj", "services/MyFoodDelivery.CustomerSvc/src/Presentation/CustomerSvc.HttpApi.Host/"]
COPY ["services/MyFoodDelivery.CustomerSvc/src/Presentation/CustomerSvc.HttpApi/CustomerSvc.HttpApi.csproj", "services/MyFoodDelivery.CustomerSvc/src/Presentation/CustomerSvc.HttpApi/"]
COPY ["services/MyFoodDelivery.CustomerSvc/src/Application/CustomerSvc.Application/CustomerSvc.Application.csproj", "services/MyFoodDelivery.CustomerSvc/src/Application/CustomerSvc.Application/"]
COPY ["services/MyFoodDelivery.CustomerSvc/src/Domain/CustomerSvc.Domain/CustomerSvc.Domain.csproj", "services/MyFoodDelivery.CustomerSvc/src/Domain/CustomerSvc.Domain/"]
COPY ["services/MyFoodDelivery.Shared/MyFoodDelivery.Shared.csproj", "services/MyFoodDelivery.Shared/"]
COPY ["services/MyFoodDelivery.CustomerSvc/src/Infrastructure/CustomerSvc.Infrastructure/CustomerSvc.Infrastructure.csproj", "services/MyFoodDelivery.CustomerSvc/src/Infrastructure/CustomerSvc.Infrastructure/"]
RUN dotnet restore "./services/MyFoodDelivery.CustomerSvc/src/Presentation/CustomerSvc.HttpApi.Host/CustomerSvc.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/MyFoodDelivery.CustomerSvc/src/Presentation/CustomerSvc.HttpApi.Host"
RUN dotnet build "./CustomerSvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CustomerSvc.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerSvc.HttpApi.Host.dll"]