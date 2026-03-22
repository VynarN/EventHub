variable "name" {
  description = "The name of the BFF App Service."
  type        = string
}

variable "location" {
  description = "The Azure region where the BFF App Service should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the BFF App Service."
  type        = string
}

variable "app_service_plan_name" {
  description = "The name of the App Service Plan for the BFF."
  type        = string
}

variable "app_service_plan_sku_tier" {
  description = "Documentation only; the plan SKU is app_service_plan_sku_size (e.g. B1 = Basic, S1 = Standard)."
  type        = string
  default     = "Basic"
}

variable "app_service_plan_sku_size" {
  description = "App Service plan SKU for the BFF, e.g. B1 (Basic), S1 (Standard)."
  type        = string
  default     = "B1"
}

variable "vnet_subnet_id" {
  description = "The ID of the subnet for VNet integration."
  type        = string
}

variable "web_api_base_url" {
  description = "The base URL of the EventHub Web API."
  type        = string
}

variable "environment_name" {
  description = "The ASPNETCORE_ENVIRONMENT value for the BFF App Service."
  type        = string
}

variable "application_insights_connection_string" {
  description = "Application Insights connection string (e.g. Key Vault reference)."
  type        = string
  sensitive   = true
}