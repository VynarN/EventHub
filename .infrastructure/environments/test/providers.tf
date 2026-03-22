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
    container_name       = "tst-tfstate"
    key                  = "tst.tfstate"
  }
}

provider "azurerm" {
  features {}
}
