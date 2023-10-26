provider "azurerm" {
  features {}
  subscription_id = "ffc2541c-9763-44de-a3a1-77de947ca18d" # PROD
}

resource "azurerm_resource_group" "gifts_prod" {
  name     = "ca.rattest.gifts.prod"
  location = "canadacentral"
}

resource "random_id" "tf" {
  byte_length = 4
}

resource "azurerm_storage_account" "tf" {
  name                     = "terraform-gifts-${random_id.tf.hex}"
  location                 = "canadacentral"
  account_tier             = "Standard"
  account_kind             = "StorageV2"
  resource_group_name      = azurerm_resource_group.gifts_prod.name
  account_replication_type = "ZRS"
}
