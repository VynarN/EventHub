#!/usr/bin/env bash
# Function App: app settings as Key Vault references (no secret values in GitHub).
# The function’s managed identity must have Key Vault Secrets User (configured in Terraform).
# Usage: apply-functionapp-kv-settings.sh <resource-group> <function-app-name> <key-vault-name>
set -euo pipefail

RG="${1:?resource group}"
APP_NAME="${2:?function app name}"
VAULT_NAME="${3:?key vault name}"

base="https://${VAULT_NAME}.vault.azure.net/secrets"

kv_ref() {
  local secret_name="$1"
  printf '@Microsoft.KeyVault(SecretUri=%s/%s/)' "$base" "$secret_name"
}

az functionapp config appsettings set \
  --resource-group "$RG" \
  --name "$APP_NAME" \
  --settings \
  "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated" \
  "CosmosDb__ConnectionString=$(kv_ref CosmosDb-ConnectionString)" \
  "ServiceBusConnection=$(kv_ref ServiceBus-ConnectionString)" \
  "APPLICATIONINSIGHTS_CONNECTION_STRING=$(kv_ref ApplicationInsights-ConnectionString)" \
  "WEBSITE_VNET_ROUTE_ALL=1"
