FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EventHub.Cosmos/EventHub.Cosmos.csproj EventHub.Cosmos/
COPY EventHub.FunctionApp/EventHub.FunctionApp.csproj EventHub.FunctionApp/
RUN dotnet restore "EventHub.FunctionApp/EventHub.FunctionApp.csproj"
COPY EventHub.Cosmos/ EventHub.Cosmos/
COPY EventHub.FunctionApp/ EventHub.FunctionApp/
WORKDIR /src/EventHub.FunctionApp
RUN dotnet publish "EventHub.FunctionApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
COPY --from=build /app/publish /home/site/wwwroot
