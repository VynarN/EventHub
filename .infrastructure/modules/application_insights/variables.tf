
variable "name" {
  description = "The name of the Application Insights instance."
  type        = string
}

variable "location" {
  description = "The Azure region where the Application Insights instance should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the Application Insights instance."
  type        = string
}

variable "application_type" {
  description = "The type of the Application Insights instance. Currently, only 'web' is supported."
  type        = string
  default     = "web"
}
