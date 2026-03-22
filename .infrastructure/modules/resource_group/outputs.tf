
output "name" {
  value       = azurerm_resource_group.main.name
  description = "The name of the resource group."
}

output "location" {
  value       = azurerm_resource_group.main.location
  description = "The location of the resource group."
}
