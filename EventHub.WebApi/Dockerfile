FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EventHub.Cosmos/EventHub.Cosmos.csproj EventHub.Cosmos/
COPY EventHub.WebApi/EventHub.WebApi.csproj EventHub.WebApi/
RUN dotnet restore "EventHub.WebApi/EventHub.WebApi.csproj"
COPY EventHub.Cosmos/ EventHub.Cosmos/
COPY EventHub.WebApi/ EventHub.WebApi/
WORKDIR /src/EventHub.WebApi
RUN dotnet publish "EventHub.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
# Bind IPv4 explicitly — Docker Desktop (Windows) port publish to [::]:8080 alone is unreliable for localhost:5080.
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventHub.WebApi.dll"]
