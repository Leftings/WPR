# Step 1: Build the .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY Backend/ ./
RUN dotnet publish -c Release -o /app/out

# Step 2: Build the React Frontend
FROM node:18 AS frontend-build
WORKDIR /frontend
COPY Frontend/ ./
RUN npm install && npm run build

# Step 3: Combine Backend and Frontend
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published .NET files from the build stage
COPY --from=build /app/out ./backend

# Copy the built React static files into the wwwroot folder
COPY --from=frontend-build /frontend/build ./wwwroot

# Set the entrypoint to the backend DLL
ENTRYPOINT ["dotnet", "backend/WPR.dll"]