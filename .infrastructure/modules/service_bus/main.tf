resource "azurerm_servicebus_namespace" "main" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
}

resource "azurerm_servicebus_queue" "event_queue" {
  name         = var.queue_name
  namespace_id = azurerm_servicebus_namespace.main.id
  # Partitioned queues require Standard or Premium; Basic namespaces must use false.
  partitioning_enabled = var.sku != "Basic"
}

resource "azurerm_private_endpoint" "servicebus_pe" {
  count               = var.enable_private_endpoint ? 1 : 0
  name                = "${var.name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.name}-psc"
    private_connection_resource_id = azurerm_servicebus_namespace.main.id
    is_manual_connection           = false
    subresource_names              = ["namespace"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.servicebus_dns[0].id]
  }
}

resource "azurerm_private_dns_zone" "servicebus_dns" {
  count               = var.enable_private_endpoint ? 1 : 0
  name                = "privatelink.servicebus.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "servicebus_dns_link" {
  count                 = var.enable_private_endpoint ? 1 : 0
  name                  = "${var.name}-servicebus-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.servicebus_dns[0].name
  virtual_network_id    = split("/subnets/", var.private_endpoint_subnet_id)[0]
}
