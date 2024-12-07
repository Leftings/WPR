FROM nginx:alpine

# Copy the wait-for-it.sh script into the container
COPY nginx/scripts/wait-for-it.sh /wait-for-it.sh

# Make it executable
RUN chmod +x /wait-for-it.sh

# Debug step: List the file to verify it's there
RUN ls -l /wait-for-it.sh

# Copy nginx config and SSL certificates
COPY nginx.conf /nginx.conf

# Copy frontend build to the Nginx container
COPY ./frontend/dist /usr/share/nginx/html

# Expose the ports for nginx
EXPOSE 8080 8443

# Set the entrypoint to wait for backend service and then start nginx
ENTRYPOINT ["/wait-for-it.sh", "backend:5001", "--", "nginx", "-g", "daemon off;"]
