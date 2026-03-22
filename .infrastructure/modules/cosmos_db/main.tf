resource "azurerm_cosmosdb_account" "main" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = var.offer_type
  kind                = var.kind

  consistency_policy {
    consistency_level       = var.consistency_policy.consistency_level
    max_interval_in_seconds = var.consistency_policy.max_interval_in_seconds
    max_staleness_prefix    = var.consistency_policy.max_staleness_prefix
  }

  geo_location {
    location          = var.geo_locations[0].location
    failover_priority = var.geo_locations[0].failover_priority
    zone_redundant    = lookup(var.geo_locations[0], "zone_redundant", null)
  }

  dynamic "geo_location" {
    for_each = length(var.geo_locations) > 1 ? slice(var.geo_locations, 1, length(var.geo_locations)) : []
    content {
      location          = geo_location.value.location
      failover_priority = geo_location.value.failover_priority
      zone_redundant    = lookup(geo_location.value, "zone_redundant", null)
    }
  }

  dynamic "capabilities" {
    for_each = var.capabilities
    content {
      name = capabilities.value
    }
  }
}

resource "azurerm_cosmosdb_sql_database" "eventhub" {
  name                = "EventHub"
  resource_group_name = azurerm_cosmosdb_account.main.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
  throughput          = 400 # Default throughput
}

resource "azurerm_cosmosdb_sql_container" "events" {
  name                = "Events"
  resource_group_name = azurerm_cosmosdb_account.main.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_sql_database.eventhub.name
  partition_key_paths = ["/Id"]
  throughput          = 400 # Default throughput
}

resource "azurerm_private_endpoint" "cosmos_pe" {
  count               = var.enable_private_endpoint ? 1 : 0
  name                = "${var.name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.name}-psc"
    private_connection_resource_id = azurerm_cosmosdb_account.main.id
    is_manual_connection           = false
    subresource_names              = ["sql"] # For SQL API
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.cosmos_dns[0].id]
  }
}

resource "azurerm_private_dns_zone" "cosmos_dns" {
  count               = var.enable_private_endpoint ? 1 : 0
  name                = "privatelink.documents.azure.com"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "cosmos_dns_link" {
  count                 = var.enable_private_endpoint ? 1 : 0
  name                  = "${var.name}-cosmos-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.cosmos_dns[0].name
  virtual_network_id    = split("/subnets/", var.private_endpoint_subnet_id)[0]
}
