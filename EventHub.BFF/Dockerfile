FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY EventHub.BFF/EventHub.BFF.csproj ./EventHub.BFF/
RUN dotnet restore ./EventHub.BFF/EventHub.BFF.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/EventHub.BFF
RUN dotnet publish -c Release -o output --no-restore

# Build Angular SPA
FROM node:20-alpine AS build-spa
WORKDIR /app
COPY eventhub-spa/ ./eventhub-spa/
WORKDIR /app/eventhub-spa
RUN npm ci
RUN npm run build -- --configuration=production --base-href=/ --deploy-url=/

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build-env /app/EventHub.BFF/output .
# Angular application builder outputs browser assets under dist/<project>/browser/
COPY --from=build-spa /app/eventhub-spa/dist/eventhub-spa/browser ./wwwroot

ENTRYPOINT ["dotnet", "EventHub.BFF.dll"]