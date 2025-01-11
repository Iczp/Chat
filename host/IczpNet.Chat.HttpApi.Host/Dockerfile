# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["IczpNet.jpg", "."]
COPY ["README.md", "."]
COPY ["host/IczpNet.Chat.HttpApi.Host/IczpNet.Chat.HttpApi.Host.csproj", "host/IczpNet.Chat.HttpApi.Host/"]
COPY ["src/IczpNet.Chat.Application/IczpNet.Chat.Application.csproj", "src/IczpNet.Chat.Application/"]
COPY ["src/IczpNet.Chat.Application.Contracts/IczpNet.Chat.Application.Contracts.csproj", "src/IczpNet.Chat.Application.Contracts/"]
COPY ["src/IczpNet.Chat.Domain.Shared/IczpNet.Chat.Domain.Shared.csproj", "src/IczpNet.Chat.Domain.Shared/"]
COPY ["src/IczpNet.Chat.Domain/IczpNet.Chat.Domain.csproj", "src/IczpNet.Chat.Domain/"]
COPY ["src/IczpNet.Chat.HttpApi/IczpNet.Chat.HttpApi.csproj", "src/IczpNet.Chat.HttpApi/"]
COPY ["src/IczpNet.Chat.EntityFrameworkCore/IczpNet.Chat.EntityFrameworkCore.csproj", "src/IczpNet.Chat.EntityFrameworkCore/"]
COPY ["host/IczpNet.Chat.Host.Shared/IczpNet.Chat.Host.Shared.csproj", "host/IczpNet.Chat.Host.Shared/"]
RUN dotnet restore "./host/IczpNet.Chat.HttpApi.Host/IczpNet.Chat.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/host/IczpNet.Chat.HttpApi.Host"
RUN dotnet build "./IczpNet.Chat.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IczpNet.Chat.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IczpNet.Chat.HttpApi.Host.dll"]