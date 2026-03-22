
output "endpoint" {
  value       = azurerm_cosmosdb_account.main.endpoint
  description = "The endpoint of the Cosmos DB account."
}

output "primary_key" {
  value       = azurerm_cosmosdb_account.main.primary_key
  description = "The primary key of the Cosmos DB account."
  sensitive   = true
}

output "connection_string" {
  value       = azurerm_cosmosdb_account.main.primary_sql_connection_string
  description = "The connection string for the Cosmos DB account."
  sensitive   = true
}
