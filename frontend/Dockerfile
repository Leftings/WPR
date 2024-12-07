#FROM node:18 AS build
#WORKDIR /app
#COPY package*.json ./
#RUN npm install
#COPY . .
#RUN npm run build

#FROM nginx:alpine
#COPY --from=build /app/dist /usr/share/nginx/html
#EXPOSE 80
#CMD ["nginx", "-g", "daemon off;"]

# Step 1: Build the .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the Backend directory to the container
COPY backend/ ../backend/

# Publish the backend .NET app
RUN dotnet publish backend/WPR.csproj -c Release -o /app/out

# Step 2: Build the React frontend using Vite
FROM node:18 AS frontend-build
WORKDIR /frontend

# Copy the Frontend directory to the container
COPY frontend/ ../frontend/

# Set the working directory to where your Vite project is located
WORKDIR /frontend/frontend

# Install dependencies and build the React app (Vite build output goes to dist/)
RUN npm install
RUN npm run build  # Builds the app and puts the static files in the 'dist' directory

# Step 3: Combine Backend and Frontend
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published .NET files from the backend build stage
COPY --from=build /app/out ./backend

# Copy the Vite build output (static files) from the frontend build stage
COPY --from=frontend-build /frontend/frontend/dist ./wwwroot

# Set the entrypoint to start the backend application
ENTRYPOINT ["dotnet", "backend/WPR.dll"]
