
output "id" {
  value       = azurerm_virtual_network.main.id
  description = "The ID of the Virtual Network."
}

output "name" {
  value       = azurerm_virtual_network.main.name
  description = "The name of the Virtual Network."
}
