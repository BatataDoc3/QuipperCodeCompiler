ARG NODE_VERSION=24.11.1-alpine
FROM node:${NODE_VERSION} AS base

WORKDIR /app

RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001 -G nodejs && \
    chown -R nodejs:nodejs /app

FROM base AS deps

COPY package*.json ./

RUN --mount=type=cache,target=/root/.npm,sharing=locked \
    npm ci --omit=dev && \
    npm cache clean --force

RUN chown -R nodejs:nodejs /app

FROM base AS build-deps

COPY package*.json ./

RUN --mount=type=cache,target=/root/.npm,sharing=locked \
    npm ci --no-audit --no-fund && \
    npm cache clean --force

RUN mkdir -p /app/node_modules/.vite && \
    chown -R nodejs:nodejs /app

FROM build-deps AS build

COPY --chown=nodejs:nodejs . .

RUN npm run build

RUN chown -R nodejs:nodejs /app

FROM build-deps AS development

ENV NODE_ENV=development \
    NPM_CONFIG_LOGLEVEL=warn

COPY . .

RUN mkdir -p /app/node_modules/.vite && \
    chown -R nodejs:nodejs /app && \
    chmod -R 775 /app

USER nodejs

EXPOSE 3000 5173 9229

CMD ["npm", "run", "dev", "--", "--host"]

ARG NODE_VERSION=24.11.1-alpine
FROM node:${NODE_VERSION} AS production

WORKDIR /app

RUN npm install -g serve

RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001 -G nodejs

COPY --from=build --chown=nodejs:nodejs /app/dist ./dist

USER nodejs

EXPOSE 3000

CMD ["serve", "-s", "dist", "-l", "3000"]