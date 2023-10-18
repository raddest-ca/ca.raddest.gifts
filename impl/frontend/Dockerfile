FROM node:18-alpine AS build

WORKDIR /app

COPY package.json ./
COPY package-lock.json ./
RUN npm ci
COPY . ./
RUN npm run build

FROM node:18-alpine
COPY --from=build /app/package*.json ./
RUN npm ci --production --ignore-scripts
COPY --from=build /app/build ./
EXPOSE 3000
CMD ["node", "./index.js"]



# FROM nginx:1.19-alpine
# COPY --from=build /app/public /usr/share/nginx/html