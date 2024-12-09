FROM nginx:alpine

COPY ./nginx/scripts/wait-for-it.sh /usr/local/bin/wait-for-it
RUN chmod +x /usr/local/bin/wait-for-it
RUN chown root:root /usr/local/bin/wait-for-it.sh

# Copy nginx config
COPY nginx.conf /nginx.conf

# Copy frontend build to the Nginx container
COPY ./../frontend/dist /usr/share/nginx/html

# Expose the ports for nginx
EXPOSE 8080 8443

ENTRYPOINT ["nginx", "-g", "daemon off;"]