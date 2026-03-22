
variable "name" {
  description = "The name of the Service Bus Namespace."
  type        = string
}

variable "location" {
  description = "The Azure region where the Service Bus Namespace should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the Service Bus Namespace."
  type        = string
}

variable "sku" {
  description = "Service Bus namespace SKU: Basic (cheapest), Standard, or Premium. Private endpoints require Premium."
  type        = string
  default     = "Basic"

  validation {
    condition     = !var.enable_private_endpoint || var.sku == "Premium"
    error_message = "Service Bus private endpoints are only supported on Premium namespaces; set sku = \"Premium\" or set enable_private_endpoint = false."
  }
}

variable "queue_name" {
  description = "The name of the Service Bus Queue."
  type        = string
  default     = "queue.1"
}

variable "enable_private_endpoint" {
  description = "When true, create private endpoint and private DNS for Service Bus. Use this flag so Terraform can evaluate count at plan time."
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "Subnet ID for the Service Bus private endpoint. Required when enable_private_endpoint is true."
  type        = string
  default     = null
}
