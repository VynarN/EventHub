resource "azurerm_storage_account" "function_app_storage" {
  name                     = var.storage_account_name
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
}

moved {
  from = azurerm_app_service_plan.main
  to   = azurerm_service_plan.main
}

resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Windows"
  sku_name            = var.app_service_plan_sku_size
}

resource "azurerm_windows_function_app" "main" {
  name                = var.name
  resource_group_name = var.resource_group_name
  location            = var.location

  service_plan_id            = azurerm_service_plan.main.id
  storage_account_name       = azurerm_storage_account.function_app_storage.name
  storage_account_access_key = azurerm_storage_account.function_app_storage.primary_access_key

  functions_extension_version = "~4"

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME" = "dotnet-isolated"
    # Trigger uses Connection = "ServiceBusConnection" — must match this setting name.
    "ServiceBusConnection" = var.service_bus_connection_string
    "CosmosDb__ConnectionString" = var.cosmos_db_connection_string
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.application_insights_connection_string
    "WEBSITE_VNET_ROUTE_ALL"                = "1"
  }

  site_config {
    vnet_route_all_enabled = true
    application_stack {
      dotnet_version = "v8.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_app_service_virtual_network_swift_connection" "vnet_integration" {
  app_service_id = azurerm_windows_function_app.main.id
  subnet_id      = var.vnet_subnet_id
}

# Storage accounts require one private endpoint per subresource (blob, queue, table, file).
resource "azurerm_private_endpoint" "storage_blob" {
  name                = "${var.storage_account_name}-pe-blob"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.storage_account_name}-psc-blob"
    private_connection_resource_id = azurerm_storage_account.function_app_storage.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.storage_blob_dns.id]
  }
}

resource "azurerm_private_endpoint" "storage_queue" {
  name                = "${var.storage_account_name}-pe-queue"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.storage_account_name}-psc-queue"
    private_connection_resource_id = azurerm_storage_account.function_app_storage.id
    is_manual_connection           = false
    subresource_names              = ["queue"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.storage_queue_dns.id]
  }
}

resource "azurerm_private_endpoint" "storage_table" {
  name                = "${var.storage_account_name}-pe-table"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.storage_account_name}-psc-table"
    private_connection_resource_id = azurerm_storage_account.function_app_storage.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.storage_table_dns.id]
  }
}

resource "azurerm_private_endpoint" "storage_file" {
  name                = "${var.storage_account_name}-pe-file"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.storage_account_name}-psc-file"
    private_connection_resource_id = azurerm_storage_account.function_app_storage.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.storage_file_dns.id]
  }
}

resource "azurerm_private_dns_zone" "storage_blob_dns" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone" "storage_queue_dns" {
  name                = "privatelink.queue.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone" "storage_table_dns" {
  name                = "privatelink.table.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone" "storage_file_dns" {
  name                = "privatelink.file.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_blob_dns_link" {
  name                  = "${var.storage_account_name}-blob-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.storage_blob_dns.name
  virtual_network_id    = split("/subnets/", var.private_endpoint_subnet_id)[0]
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_queue_dns_link" {
  name                  = "${var.storage_account_name}-queue-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.storage_queue_dns.name
  virtual_network_id    = split("/subnets/", var.private_endpoint_subnet_id)[0]
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_table_dns_link" {
  name                  = "${var.storage_account_name}-table-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.storage_table_dns.name
  virtual_network_id    = split("/subnets/", var.private_endpoint_subnet_id)[0]
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_file_dns_link" {
  name                  = "${var.storage_account_name}-file-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.storage_file_dns.name
  virtual_network_id    = split("/subnets/", var.private_endpoint_subnet_id)[0]
}
