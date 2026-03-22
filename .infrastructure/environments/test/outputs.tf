output "key_vault_id" {
  description = "Key Vault resource ID."
  value       = module.key_vault.id
}

output "key_vault_name" {
  description = "Key Vault name (use with Azure CLI: az keyvault secret show --vault-name ...)."
  value       = module.key_vault.name
}

output "key_vault_uri" {
  description = "Key Vault data-plane URI."
  value       = module.key_vault.vault_uri
}

output "key_vault_resource_group_name" {
  description = "Resource group containing the Key Vault."
  value       = module.resource_group.name
}

output "key_vault_secret_names" {
  description = "Map of logical keys to Key Vault secret names for GitHub Actions / scripts (fetch values with Key Vault Secrets User + az or Azure/get-keyvault-secrets)."
  value = {
    cosmos_db_connection_string              = azurerm_key_vault_secret.cosmos_db_connection_string.name
    cosmos_db_primary_key                    = azurerm_key_vault_secret.cosmos_db_primary_key.name
    service_bus_connection_string            = azurerm_key_vault_secret.service_bus_connection_string.name
    application_insights_connection_string   = azurerm_key_vault_secret.application_insights_connection_string.name
    application_insights_instrumentation_key = azurerm_key_vault_secret.application_insights_instrumentation_key.name
    function_app_storage_connection_string   = azurerm_key_vault_secret.function_app_storage_connection_string.name
  }
}

output "web_api_default_host_name" {
  description = "Default hostname of the Web API App Service."
  value       = module.web_api_app_service.default_host_name
}

output "bff_default_host_name" {
  description = "Default hostname of the BFF App Service."
  value       = module.bff_app_service.default_host_name
}
