FROM nginx:alpine

RUN apk add --no-cache bash
# Copy the wait-for-it script into the container
COPY ./nginx/scripts/wait-for-it.sh /usr/local/bin/wait-for-it
RUN chmod +x /usr/local/bin/wait-for-it

# Copy nginx config
COPY nginx.conf /nginx.conf

# Copy frontend build to Nginx container
COPY ./../frontend/dist /usr/share/nginx/html

# Expose the necessary ports
EXPOSE 8080 8443

# Use wait-for-it to ensure the backend service is ready before starting Nginx
# Replace 'backend:5000' with the actual host and port of your service.
ENTRYPOINT ["/bin/bash", "-c", "/usr/local/bin/wait-for-it backend:5000 -- nginx -g 'daemon off;'"]
