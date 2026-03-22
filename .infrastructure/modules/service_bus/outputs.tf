
output "namespace_name" {
  value       = azurerm_servicebus_namespace.main.name
  description = "The name of the Service Bus Namespace."
}

output "primary_connection_string" {
  value       = azurerm_servicebus_namespace.main.default_primary_connection_string
  description = "The primary connection string for the Service Bus Namespace."
  sensitive   = true
}
