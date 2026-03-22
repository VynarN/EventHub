output "app_service_name" {
  value       = azurerm_linux_web_app.main.name
  description = "The name of the App Service."
}

output "app_service_id" {
  value       = azurerm_linux_web_app.main.id
  description = "The ID of the App Service."
}

output "default_host_name" {
  value       = azurerm_linux_web_app.main.default_hostname
  description = "The default host name of the App Service."
}

output "principal_id" {
  value       = azurerm_linux_web_app.main.identity[0].principal_id
  description = "Object ID of the App Service system-assigned managed identity."
}
