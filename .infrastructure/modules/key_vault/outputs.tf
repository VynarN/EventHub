output "id" {
  description = "Resource ID of the Key Vault."
  value       = azurerm_key_vault.main.id
}

output "name" {
  description = "Name of the Key Vault."
  value       = azurerm_key_vault.main.name
}

output "vault_uri" {
  description = "URI of the Key Vault (data plane)."
  value       = azurerm_key_vault.main.vault_uri
}
