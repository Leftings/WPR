# Use the official Nginx image as the base
FROM nginx:alpine

# Install bash for the wait-for-it script and curl for debugging purposes
RUN apk add --no-cache bash curl

# Copy the wait-for-it.sh script into the container
COPY nginx/scripts/wait-for-it.sh /usr/local/bin/wait-for-it.sh

# Make the wait-for-it.sh script executable
RUN chmod +x /usr/local/bin/wait-for-it.sh

# Verify the file is correctly copied and executable (for debugging purposes)
RUN ls -l /usr/local/bin/wait-for-it.sh

# Copy the nginx.conf file into the container
COPY nginx.conf /etc/nginx/nginx.conf

# Copy the frontend build to the Nginx container
COPY ./frontend/dist /usr/share/nginx/html

# Expose the ports for nginx
EXPOSE 8080 8443

# Set the entrypoint to wait for the backend service and then start nginx
ENTRYPOINT ["/usr/local/bin/wait-for-it.sh", "backend:5001", "--", "nginx", "-g", "daemon off;"]

# Optional: Add health check to verify if Nginx is running properly
HEALTHCHECK --interval=30s --timeout=30s --retries=3 CMD curl --fail http://localhost:8080 || exit 1
