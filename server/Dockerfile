FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workspace

COPY ["src/OpenAuth.Server/OpenAuth.Server.csproj", "src/OpenAuth.Server/"]
COPY ["src/OpenAuth.Application/OpenAuth.Application.csproj", "src/OpenAuth.Application/"]
COPY ["src/OpenAuth.Domain/OpenAuth.Domain.csproj", "src/OpenAuth.Domain/"]
COPY ["src/OpenAuth.Infrastructure/OpenAuth.Infrastructure.csproj", "src/OpenAuth.Infrastructure/"]

RUN dotnet restore "src/OpenAuth.Server/OpenAuth.Server.csproj"

# Copy the remaining source
COPY . .

WORKDIR /workspace/src/OpenAuth.Server
RUN dotnet build "OpenAuth.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenAuth.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OpenAuth.Server.dll"]