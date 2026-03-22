output "function_app_name" {
  value       = azurerm_windows_function_app.main.name
  description = "The name of the Function App."
}

output "function_app_id" {
  value       = azurerm_windows_function_app.main.id
  description = "The ID of the Function App."
}

output "function_app_storage_account_name" {
  value       = azurerm_storage_account.function_app_storage.name
  description = "The name of the Function App's storage account."
}

output "storage_account_primary_connection_string" {
  value       = azurerm_storage_account.function_app_storage.primary_connection_string
  sensitive   = true
  description = "Primary connection string for the Function host storage account (AzureWebJobsStorage)."
}

output "principal_id" {
  value       = azurerm_windows_function_app.main.identity[0].principal_id
  description = "Object ID of the Function App system-assigned managed identity."
}
