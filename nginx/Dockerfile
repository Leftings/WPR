# Use the official Nginx image from Docker Hub
FROM nginx:latest

# Copy the custom Nginx configuration file
COPY nginx.conf /etc/nginx/nginx.conf

# Copy frontend build files into the Nginx HTML directory
COPY ./frontend/dist /usr/share/nginx/html

# Expose the necessary ports
EXPOSE 80 443

# Set the entrypoint to run nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]
