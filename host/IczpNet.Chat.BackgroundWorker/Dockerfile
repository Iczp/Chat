FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["common.props", "."]
COPY ["IczpNet.jpg", "."]
COPY ["README.md", "."]
COPY ["host/IczpNet.Chat.BackgroundWorker/IczpNet.Chat.BackgroundWorker.csproj", "host/IczpNet.Chat.BackgroundWorker/"]
COPY ["host/IczpNet.Chat.Host.Shared/IczpNet.Chat.Host.Shared.csproj", "host/IczpNet.Chat.Host.Shared/"]
COPY ["src/IczpNet.Chat.Application/IczpNet.Chat.Application.csproj", "src/IczpNet.Chat.Application/"]
COPY ["src/IczpNet.Chat.Application.Contracts/IczpNet.Chat.Application.Contracts.csproj", "src/IczpNet.Chat.Application.Contracts/"]
COPY ["src/IczpNet.Chat.Domain/IczpNet.Chat.Domain.csproj", "src/IczpNet.Chat.Domain/"]
COPY ["src/IczpNet.Chat.Domain.Shared/IczpNet.Chat.Domain.Shared.csproj", "src/IczpNet.Chat.Domain.Shared/"]
COPY ["src/IczpNet.Chat.EntityFrameworkCore/IczpNet.Chat.EntityFrameworkCore.csproj", "src/IczpNet.Chat.EntityFrameworkCore/"]
RUN dotnet restore "host/IczpNet.Chat.BackgroundWorker/IczpNet.Chat.BackgroundWorker.csproj"
COPY . .
WORKDIR /src/host/IczpNet.Chat.BackgroundWorker
RUN dotnet publish "IczpNet.Chat.BackgroundWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IczpNet.Chat.BackgroundWorker.dll"]
