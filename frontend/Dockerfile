# Step 1: Build the frontend with Node.js and Vite
FROM node:18 AS build

WORKDIR /app

# Copy package.json and package-lock.json from the frontend directory
COPY frontend/package.json frontend/package-lock.json ./

# Install dependencies using npm ci (clean install)
RUN npm ci

# Copy the rest of the application files (including source code)
COPY frontend ./

# Run the build script using Vite
RUN npm run build

# Step 2: Set up Nginx to serve the static files
FROM nginx:alpine

# Copy the built files from the build stage to Nginx's default directory
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80 for Nginx to serve the app
EXPOSE 3000

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]
