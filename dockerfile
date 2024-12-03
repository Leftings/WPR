# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY Backend/ ./
RUN dotnet publish -c Release -o out

# Build Frontend
FROM node:18 AS frontend-build
WORKDIR /frontend
COPY Frontend/ ./
RUN npm install && npm run build

# Combine Backend and Frontend
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./backend
COPY --from=frontend-build /frontend/build ./wwwroot
ENTRYPOINT ["dotnet", "backend\bin\Debug\net8.0\WPR.dll"]
