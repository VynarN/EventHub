#!/usr/bin/env bash
# Web API: App Service settings as Key Vault references (no secret values in GitHub).
# The site’s managed identity must have Key Vault Secrets User (configured in Terraform).
# Usage: apply-webapi-app-service-kv-settings.sh <resource-group> <app-name> <key-vault-name>
set -euo pipefail

RG="${1:?resource group}"
APP_NAME="${2:?app name}"
VAULT_NAME="${3:?key vault name}"

base="https://${VAULT_NAME}.vault.azure.net/secrets"

kv_ref() {
  local secret_name="$1"
  printf '@Microsoft.KeyVault(SecretUri=%s/%s/)' "$base" "$secret_name"
}

az webapp config appsettings set \
  --resource-group "$RG" \
  --name "$APP_NAME" \
  --settings \
  "CosmosDb__ConnectionString=$(kv_ref CosmosDb-ConnectionString)" \
  "ServiceBus__ConnectionString=$(kv_ref ServiceBus-ConnectionString)" \
  "APPLICATIONINSIGHTS_CONNECTION_STRING=$(kv_ref ApplicationInsights-ConnectionString)"
