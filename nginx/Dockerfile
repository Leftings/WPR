FROM nginx:alpine

RUN apk --no-cache add bash
# Copy the wait-for-it script into the container
COPY ./nginx/scripts/wait-for-it.sh /usr/local/bin/wait-for-it
RUN chmod +x /usr/local/bin/wait-for-it

# Copy nginx config
COPY nginx.conf /nginx.conf

# Copy frontend build to Nginx container
COPY ./../frontend/dist /usr/share/nginx/html

# Expose the necessary ports
EXPOSE 8080 8443

# Use ash to run the wait-for-it script (since bash is not required)
ENTRYPOINT ["/bin/bash", "-c", "/usr/local/bin/wait-for-it backend:5000 -- nginx -g 'daemon off;'"]
