
output "instrumentation_key" {
  value       = azurerm_application_insights.main.instrumentation_key
  description = "The Instrumentation Key for the Application Insights instance."
  sensitive   = true
}

output "connection_string" {
  value       = azurerm_application_insights.main.connection_string
  description = "The Connection String for the Application Insights instance."
  sensitive   = true
}
