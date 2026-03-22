resource "azurerm_subnet" "main" {
  name                 = var.name
  resource_group_name  = var.resource_group_name
  virtual_network_name = var.virtual_network_name
  address_prefixes     = var.address_prefixes
  service_endpoints    = var.service_endpoints

  dynamic "delegation" {
    for_each = var.delegations
    content {
      name = delegation.value.name
      service_delegation {
        name    = delegation.value.service_delegation_name
        actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
    }
  }
}
