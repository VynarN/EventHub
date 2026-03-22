#!/usr/bin/env bash
# BFF: App Service settings — Key Vault reference for App Insights + public env vars for YARP / ASP.NET.
# Requires ASPNETCORE_ENVIRONMENT and WEB_API_PUBLIC_URL (e.g. GitHub Environment variables).
# Usage: apply-bff-app-service-kv-settings.sh <resource-group> <app-name> <key-vault-name>
set -euo pipefail

RG="${1:?resource group}"
APP_NAME="${2:?app name}"
VAULT_NAME="${3:?key vault name}"

if [[ -z "${ASPNETCORE_ENVIRONMENT:-}" || -z "${WEB_API_PUBLIC_URL:-}" ]]; then
  echo "ASPNETCORE_ENVIRONMENT and WEB_API_PUBLIC_URL must be set (GitHub Environment variables)." >&2
  exit 1
fi

base="https://${VAULT_NAME}.vault.azure.net/secrets"

kv_ref() {
  local secret_name="$1"
  printf '@Microsoft.KeyVault(SecretUri=%s/%s/)' "$base" "$secret_name"
}

az webapp config appsettings set \
  --resource-group "$RG" \
  --name "$APP_NAME" \
  --settings \
  "ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}" \
  "ReverseProxy__Clusters__webApiCluster__Destinations__default__Address=${WEB_API_PUBLIC_URL}" \
  "APPLICATIONINSIGHTS_CONNECTION_STRING=$(kv_ref ApplicationInsights-ConnectionString)"
