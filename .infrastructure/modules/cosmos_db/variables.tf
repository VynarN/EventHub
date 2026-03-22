
variable "name" {
  description = "The name of the Cosmos DB account."
  type        = string
}

variable "location" {
  description = "The Azure region where the Cosmos DB account should be created."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group in which to create the Cosmos DB account."
  type        = string
}

variable "offer_type" {
  description = "The offer type for the Cosmos DB account. Currently, only 'Standard' is supported."
  type        = string
  default     = "Standard"
}

variable "kind" {
  description = "The kind of Cosmos DB account. Can be 'GlobalDocumentDB', 'MongoDB', 'Parse', 'Cassandra', 'Table', or 'Gremlin'."
  type        = string
  default     = "GlobalDocumentDB"
}

variable "capabilities" {
  description = "A list of capabilities to enable for this Cosmos DB account. Possible values are 'EnableAggregationPipeline', 'EnableMongo '...'."
  type        = list(string)
  default     = []
}

variable "consistency_policy" {
  description = "The consistency policy for the Cosmos DB account."
  type = object({
    consistency_level       = string
    max_interval_in_seconds = optional(number)
    max_staleness_prefix    = optional(number)
  })
  default = {
    consistency_level = "Session"
  }
}

variable "geo_locations" {
  description = "A list of geographic locations where data is replicated."
  type = list(object({
    location          = string
    failover_priority = number
    zone_redundant    = optional(bool)
  }))
}

variable "enable_private_endpoint" {
  description = "When true, create private endpoint and private DNS for Cosmos. Use this flag (not a null check on subnet id) so Terraform can evaluate count at plan time."
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "Subnet ID for the Cosmos private endpoint. Required when enable_private_endpoint is true."
  type        = string
  default     = null
}
