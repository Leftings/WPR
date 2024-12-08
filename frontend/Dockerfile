# Step 1: Build the frontend with node:18
FROM node:18 AS build

WORKDIR /app

# Copy package.json and package-lock.json first to install dependencies
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci --registry=https://registry.npmjs.org/

# Copy the rest of the application
COPY . .

# Step 2: Run the build script
RUN npm run build

# Step 3: Set up Nginx to serve the static files
FROM nginx:alpine

# Copy the built files from the build stage to Nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80 for Nginx
EXPOSE 80
