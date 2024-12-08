FROM node:18 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install -D vite@5.4.10 --registry=https://registry.npmjs.org/
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
