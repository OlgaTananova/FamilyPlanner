FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 80

# Copy all csproj files and restore as distinct layers. 
COPY FamilyPlanner.sln  FamilyPlanner.sln
COPY src/CatalogService/CatalogService.csproj src/CatalogService/CatalogService.csproj
COPY src/ShoppingListService/ShoppingListService.csproj src/ShoppingListService/ShoppingListService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/NotificationService/NotificationService.csproj src/NotificationService/NotificationService.csproj

COPY tests/CatalogService.UnitTests/CatalogService.UnitTests.csproj tests/CatalogService.UnitTests/CatalogService.UnitTests.csproj
COPY tests/CatalogService.IntegrationTests/CatalogService.IntegrationTests.csproj tests/CatalogService.IntegrationTests/CatalogService.IntegrationTests.csproj
COPY tests/ShoppingListService.IntegrationTests/ShoppingListService.IntegrationTests.csproj tests/ShoppingListService.IntegrationTests/ShoppingListService.IntegrationTests.csproj
COPY tests/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj tests/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj

# Restore packages
RUN dotnet restore FamilyPlanner.sln

# Copy the app foleders
COPY src/ShoppingListService src/ShoppingListService
COPY src/Contracts src/Contracts
WORKDIR /app/src/ShoppingListService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet", "ShoppingListService.dll"]