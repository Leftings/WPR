FROM node:18 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install -D vite --registry=https://registry.yarnpkg.com
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
