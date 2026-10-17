# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AulaPedidos.Api/AulaPedidos.Api.csproj", "src/AulaPedidos.Api/"]
COPY ["src/AulaPedidos.Application/AulaPedidos.Application.csproj", "src/AulaPedidos.Application/"]
COPY ["src/AulaPedidos.Domain/AulaPedidos.Domain.csproj", "src/AulaPedidos.Domain/"]
COPY ["src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj", "src/AulaPedidos.Infrastructure/"]
RUN dotnet restore "./src/AulaPedidos.Api/AulaPedidos.Api.csproj"
COPY . .
WORKDIR "/src/src/AulaPedidos.Api"
RUN dotnet build "./AulaPedidos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AulaPedidos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AulaPedidos.Api.dll"]