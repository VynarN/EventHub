terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "eventhubrg"
    storage_account_name = "nveventhubsa"
    container_name       = "dev-tfstate"
    key                  = "dev.tfstate"
  }
}

provider "azurerm" {
  features {}
}
