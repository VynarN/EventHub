variable "name" {
  description = "Globally unique Key Vault name (3–24 alphanumeric characters only)."
  type        = string
}

variable "location" {
  description = "Azure region for the Key Vault."
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name containing the Key Vault."
  type        = string
}

variable "tenant_id" {
  description = "Azure AD tenant ID for the Key Vault."
  type        = string
}

variable "terraform_object_id" {
  description = "Object ID of the principal that runs Terraform (must be able to create secrets). Granted full secret management on the vault via access policy — no RBAC role assignment needed."
  type        = string
}

variable "soft_delete_retention_days" {
  description = "Soft-delete retention in days."
  type        = number
  default     = 7
}

variable "purge_protection_enabled" {
  description = "Enable purge protection (recommended for production; blocks immediate purge after delete)."
  type        = bool
  default     = false
}
