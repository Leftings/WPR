# Stage 1: Build the React frontend
FROM node:18 AS react-build
WORKDIR /frontend

# Install dependencies and build the React app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm install
COPY frontend/ ./
RUN npm run build  # Build the React app (static files)

# Stage 2: Build and configure the .NET backend
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 8443 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /backend
COPY ["backend/WPR.csproj", "./"]
RUN dotnet restore "backend/WPR.csproj"
COPY . .
WORKDIR "/backend/"
RUN dotnet build "WPR.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend/WPR.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Combine React frontend and .NET backend
FROM base AS final
WORKDIR /app

# Copy published .NET files
COPY --from=publish /app/publish .

# Copy the built React files from the react-build stage
COPY --from=react-build /frontend/build ./wwwroot

ENTRYPOINT ["dotnet", "WPR.dll"]
