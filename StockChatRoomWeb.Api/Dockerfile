# Multi-stage build for API with Vue.js frontend

# Build Frontend
FROM node:22-alpine AS frontend-build
WORKDIR /app/frontend
COPY StockChatRoomWeb.Frontend/package*.json ./
RUN npm install
COPY StockChatRoomWeb.Frontend/ ./
RUN npm run build -- --outDir dist

# Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln ./
COPY StockChatRoomWeb.Api/StockChatRoomWeb.Api.csproj StockChatRoomWeb.Api/
COPY StockChatRoomWeb.Core/StockChatRoomWeb.Core.csproj StockChatRoomWeb.Core/
COPY StockChatRoomWeb.Infrastructure/StockChatRoomWeb.Infrastructure.csproj StockChatRoomWeb.Infrastructure/
COPY StockChatRoomWeb.Shared/StockChatRoomWeb.Shared.csproj StockChatRoomWeb.Shared/
RUN dotnet restore StockChatRoomWeb.Api/StockChatRoomWeb.Api.csproj

COPY . ./
COPY --from=frontend-build /app/frontend/dist/ ./StockChatRoomWeb.Api/wwwroot/

RUN dotnet publish StockChatRoomWeb.Api/StockChatRoomWeb.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "StockChatRoomWeb.Api.dll"]