terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

# Per-environment stacks live under environments/<name>/. Module `source` paths cannot
# use variables at the root module; run init/plan/apply from an environment directory.
