locals {
  aspnetcore_environment = (
    var.environment == "dev" ? "Development"
    : var.environment == "test" ? "Staging"
    : "Production"
  )
  key_vault_name = coalesce(var.key_vault_name, "eh${var.environment}hubkv")
}

data "azurerm_client_config" "current" {}

# Resource Group
module "resource_group" {
  source   = "../../modules/resource_group"
  name     = "eh-${var.environment}-rg"
  location = var.location
}

# Virtual Network
module "vnet" {
  source              = "../../modules/virtual_network"
  name                = "eh-${var.environment}-vnet"
  location            = module.resource_group.location
  resource_group_name = module.resource_group.name
  address_space       = ["10.0.0.0/16"]
}

# Subnets
module "app_subnet" {
  source               = "../../modules/subnets"
  name                 = "app-subnet"
  resource_group_name  = module.resource_group.name
  virtual_network_name = module.vnet.name
  address_prefixes     = ["10.0.1.0/24"]
  delegations = [
    {
      name                    = "appservice"
      service_delegation_name = "Microsoft.Web/serverFarms"
    },
  ]
}

module "private_endpoint_subnet" {
  source               = "../../modules/subnets"
  name                 = "private-endpoint-subnet"
  resource_group_name  = module.resource_group.name
  virtual_network_name = module.vnet.name
  address_prefixes     = ["10.0.2.0/24"]
  # Private Endpoint subnets do not support service endpoints
}

# Cosmos DB
module "cosmos_db" {
  source                     = "../../modules/cosmos_db"
  name                       = "eh-${var.environment}-cosmos"
  location                   = module.resource_group.location
  resource_group_name        = module.resource_group.name
  enable_private_endpoint    = true
  private_endpoint_subnet_id = module.private_endpoint_subnet.id
  geo_locations = [
    {
      location          = module.resource_group.location
      failover_priority = 0
    },
  ]
}

# Service Bus (before apps; connection strings are stored in Key Vault)
module "service_bus" {
  source                  = "../../modules/service_bus"
  name                    = "eh-${var.environment}-sbus"
  location                = module.resource_group.location
  resource_group_name     = module.resource_group.name
  sku                     = "Basic"
  enable_private_endpoint = false
}

# Application Insights (connection string stored in Key Vault below)
module "app_insights" {
  source              = "../../modules/application_insights"
  name                = "eh-${var.environment}-ai"
  location            = module.resource_group.location
  resource_group_name = module.resource_group.name
}

# Key Vault (access policies): Terraform + app identities get secret access without RBAC role assignments
# (avoids Microsoft.Authorization/roleAssignments/write, which Contributor-only SPs lack).
module "key_vault" {
  source                = "../../modules/key_vault"
  name                  = local.key_vault_name
  location              = module.resource_group.location
  resource_group_name   = module.resource_group.name
  tenant_id             = data.azurerm_client_config.current.tenant_id
  terraform_object_id   = data.azurerm_client_config.current.object_id
}

resource "azurerm_key_vault_access_policy" "pipeline_secrets" {
  for_each     = toset(var.key_vault_secrets_user_principal_ids)
  key_vault_id = module.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = each.value

  secret_permissions = ["Get", "List"]
}

# Credentials are written to Key Vault at apply for App Service Key Vault references and for CI/CD.
# Optional: key_vault_secrets_user_principal_ids — Get/List for identities that read secrets outside Terraform.
resource "azurerm_key_vault_secret" "cosmos_db_connection_string" {
  name         = "CosmosDb-ConnectionString"
  key_vault_id = module.key_vault.id
  value        = module.cosmos_db.connection_string
  depends_on = [
    module.cosmos_db,
    module.key_vault,
  ]
}

resource "azurerm_key_vault_secret" "service_bus_connection_string" {
  name         = "ServiceBus-ConnectionString"
  key_vault_id = module.key_vault.id
  value        = module.service_bus.primary_connection_string
  depends_on = [
    module.service_bus,
    module.key_vault,
  ]
}

resource "azurerm_key_vault_secret" "application_insights_connection_string" {
  name         = "ApplicationInsights-ConnectionString"
  key_vault_id = module.key_vault.id
  value        = module.app_insights.connection_string
  depends_on = [
    module.app_insights,
    module.key_vault,
  ]
}

resource "azurerm_key_vault_secret" "application_insights_instrumentation_key" {
  name         = "ApplicationInsights-InstrumentationKey"
  key_vault_id = module.key_vault.id
  value        = module.app_insights.instrumentation_key
  depends_on = [
    module.app_insights,
    module.key_vault,
  ]
}

resource "azurerm_key_vault_secret" "cosmos_db_primary_key" {
  name         = "CosmosDb-PrimaryKey"
  key_vault_id = module.key_vault.id
  value        = module.cosmos_db.primary_key
  depends_on = [
    module.cosmos_db,
    module.key_vault,
  ]
}

# Function App
module "function_app" {
  source                                 = "../../modules/function_app"
  name                                   = "eh-${var.environment}-func"
  location                               = module.resource_group.location
  resource_group_name                    = module.resource_group.name
  app_service_plan_name                  = "eh-${var.environment}-asp-func"
  app_service_plan_sku_size              = "B1"
  storage_account_name                   = "eh${var.environment}funcsa"
  service_bus_connection_string          = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.service_bus_connection_string.versionless_id})"
  cosmos_db_connection_string            = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.cosmos_db_connection_string.versionless_id})"
  application_insights_connection_string = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.application_insights_connection_string.versionless_id})"
  vnet_subnet_id                         = module.app_subnet.id
  private_endpoint_subnet_id             = module.private_endpoint_subnet.id

  depends_on = [
    azurerm_key_vault_secret.cosmos_db_connection_string,
    azurerm_key_vault_secret.service_bus_connection_string,
    azurerm_key_vault_secret.application_insights_connection_string,
  ]
}

# Web API App Service
module "web_api_app_service" {
  source                                 = "../../modules/app_service_webapi"
  name                                   = "eh-${var.environment}-webapi"
  location                               = module.resource_group.location
  resource_group_name                    = module.resource_group.name
  app_service_plan_name                  = "eh-${var.environment}-asp-webapi"
  vnet_subnet_id                         = module.app_subnet.id
  cosmos_db_connection_string            = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.cosmos_db_connection_string.versionless_id})"
  service_bus_connection_string          = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.service_bus_connection_string.versionless_id})"
  application_insights_connection_string = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.application_insights_connection_string.versionless_id})"
  app_service_plan_sku_tier              = "Basic"
  app_service_plan_sku_size              = "B1"

  depends_on = [
    azurerm_key_vault_secret.cosmos_db_connection_string,
    azurerm_key_vault_secret.service_bus_connection_string,
    azurerm_key_vault_secret.application_insights_connection_string,
  ]
}

resource "azurerm_key_vault_access_policy" "function_app" {
  key_vault_id = module.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = module.function_app.principal_id

  secret_permissions = ["Get", "List"]
}

resource "azurerm_key_vault_access_policy" "web_api" {
  key_vault_id = module.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = module.web_api_app_service.principal_id

  secret_permissions = ["Get", "List"]
}

# BFF App Service
module "bff_app_service" {
  source                                 = "../../modules/app_service_bff"
  name                                   = "eh-${var.environment}-bff"
  location                               = module.resource_group.location
  resource_group_name                    = module.resource_group.name
  app_service_plan_name                  = "eh-${var.environment}-asp-bff"
  vnet_subnet_id                         = module.app_subnet.id
  web_api_base_url                       = "https://${module.web_api_app_service.default_host_name}"
  environment_name                       = local.aspnetcore_environment
  application_insights_connection_string = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.application_insights_connection_string.versionless_id})"
  app_service_plan_sku_tier              = "Basic"
  app_service_plan_sku_size              = "B1"

  depends_on = [
    azurerm_key_vault_secret.application_insights_connection_string,
  ]
}

resource "azurerm_key_vault_access_policy" "bff" {
  key_vault_id = module.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = module.bff_app_service.principal_id

  secret_permissions = ["Get", "List"]
}

resource "azurerm_key_vault_secret" "function_app_storage_connection_string" {
  name         = "FunctionApp-StorageConnectionString"
  key_vault_id = module.key_vault.id
  value        = module.function_app.storage_account_primary_connection_string
  depends_on = [
    module.function_app,
    module.key_vault,
  ]
}
