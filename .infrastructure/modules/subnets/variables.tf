
variable "name" {
  description = "The name of the subnet."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the subnet."
  type        = string
}

variable "virtual_network_name" {
  description = "The name of the Virtual Network in which to create the subnet."
  type        = string
}

variable "address_prefixes" {
  description = "The address prefixes for the subnet."
  type        = list(string)
}

variable "service_endpoints" {
  description = "A list of Service Endpoints to associate with the subnet."
  type        = list(string)
  default     = []
}

variable "delegations" {
  description = "Subnet delegations (e.g. Microsoft.Web/serverFarms for App Service / Functions VNet integration). Cannot combine with service_endpoints on the same subnet in Azure."
  type = list(object({
    name                    = string
    service_delegation_name = string
  }))
  default = []
}
