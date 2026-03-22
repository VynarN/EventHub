
variable "name" {
  description = "The name of the Virtual Network."
  type        = string
}

variable "location" {
  description = "The Azure region where the Virtual Network should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the Virtual Network."
  type        = string
}

variable "address_space" {
  description = "The address space of the Virtual Network."
  type        = list(string)
}
