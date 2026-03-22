
variable "name" {
  description = "The name of the App Service."
  type        = string
}

variable "location" {
  description = "The Azure region where the App Service should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the App Service."
  type        = string
}

variable "app_service_plan_name" {
  description = "The name of the App Service Plan."
  type        = string
}

variable "app_service_plan_sku_tier" {
  description = "Documentation only; the plan SKU is app_service_plan_sku_size (e.g. B1 = Basic, S1 = Standard)."
  type        = string
  default     = "Basic"
}

variable "app_service_plan_sku_size" {
  description = "App Service plan SKU, e.g. B1 (Basic), S1 (Standard). VNet integration requires a supported dedicated size."
  type        = string
  default     = "B1"
}

variable "vnet_subnet_id" {
  description = "The ID of the subnet for VNet integration."
  type        = string
}

variable "cosmos_db_connection_string" {
  description = "The connection string for the Cosmos DB."
  type        = string
  sensitive   = true
}

variable "service_bus_connection_string" {
  description = "The connection string for the Service Bus."
  type        = string
  sensitive   = true
}

variable "application_insights_connection_string" {
  description = "Application Insights connection string (e.g. Key Vault reference)."
  type        = string
  sensitive   = true
}
