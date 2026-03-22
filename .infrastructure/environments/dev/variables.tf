variable "environment" {
  description = "The deployment environment (e.g., dev, test, prod)"
  type        = string
}

variable "location" {
  description = "The Azure region for resources"
  type        = string
}

variable "key_vault_name" {
  description = "Optional Key Vault name override (3–24 alphanumeric characters, globally unique). If null, a default per environment is used."
  type        = string
  default     = null
}

variable "key_vault_secrets_user_principal_ids" {
  description = "Azure AD object IDs to grant Key Vault secret Get/List via access policy (e.g. CI identity that reads secrets with Azure CLI). Empty = none."
  type        = list(string)
  default     = []
}
