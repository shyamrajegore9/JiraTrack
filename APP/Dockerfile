FROM node:22-alpine AS build
WORKDIR /app
COPY APP/package*.json ./
RUN npm ci
COPY APP/ ./
RUN npm run build -- --configuration=production

FROM nginx:1.27-alpine AS final
COPY APP/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/APP/browser /usr/share/nginx/html
EXPOSE 80
