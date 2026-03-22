moved {
  from = azurerm_app_service_plan.main
  to   = azurerm_service_plan.main
}

resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku_size
}

resource "azurerm_linux_web_app" "main" {
  name                = var.name
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    always_on              = true
    vnet_route_all_enabled = true
    application_stack {
      dotnet_version = "8.0"
    }
  }

  # Linux App Service rejects ':' in setting names; '__' maps to ':' in ASP.NET Core configuration.
  app_settings = {
    "CosmosDb__ConnectionString"             = var.cosmos_db_connection_string
    "ServiceBus__ConnectionString"           = var.service_bus_connection_string
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.application_insights_connection_string
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_app_service_virtual_network_swift_connection" "vnet_integration" {
  app_service_id = azurerm_linux_web_app.main.id
  subnet_id      = var.vnet_subnet_id
}
