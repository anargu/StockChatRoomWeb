FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app
COPY *.sln ./
COPY StooqBot/StooqBot.csproj StooqBot/
COPY StockChatRoomWeb.Shared/StockChatRoomWeb.Shared.csproj StockChatRoomWeb.Shared/
RUN dotnet restore StooqBot/StooqBot.csproj

COPY . ./
RUN dotnet publish StooqBot/StooqBot.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "StooqBot.dll"]