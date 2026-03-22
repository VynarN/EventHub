
variable "name" {
  description = "The name of the Function App."
  type        = string
}

variable "location" {
  description = "The Azure region where the Function App should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the Function App."
  type        = string
}

variable "app_service_plan_name" {
  description = "The name of the App Service Plan."
  type        = string
}

variable "app_service_plan_sku_tier" {
  description = "Unused by Terraform (plan SKU is set via app_service_plan_sku_size). For Windows dedicated functions use Basic/Standard sizes (e.g. B1, S1)."
  type        = string
  default     = "Basic"
}

variable "app_service_plan_sku_size" {
  description = "App Service plan SKU for this Windows Function App (dedicated). Use B1 for Basic, S1 for Standard, etc."
  type        = string
  default     = "B1"
}

variable "storage_account_name" {
  description = "The name of the Storage Account for the Function App."
  type        = string
}

variable "storage_account_tier" {
  description = "The Tier for the Storage Account. e.g., 'Standard', 'Premium'."
  type        = string
  default     = "Standard"
}

variable "storage_account_replication_type" {
  description = "The Replication Type for the Storage Account. e.g., 'LRS', 'GRS', 'RAGRS', 'ZRS', 'GZRS', 'RAGZRS'."
  type        = string
  default     = "LRS"
}

variable "service_bus_connection_string" {
  description = "The connection string for the Service Bus."
  type        = string
  sensitive   = true
}

variable "cosmos_db_connection_string" {
  description = "The connection string for the Cosmos DB."
  type        = string
  sensitive   = true
}

variable "application_insights_connection_string" {
  description = "Application Insights connection string (e.g. Key Vault reference)."
  type        = string
  sensitive   = true
}

variable "vnet_subnet_id" {
  description = "The ID of the subnet for VNet integration."
  type        = string
}

variable "private_endpoint_subnet_id" {
  description = "The ID of the subnet where the storage account private endpoint will be created."
  type        = string
}
