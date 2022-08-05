resource "azurerm_resource_provider_registration" "key-vault-registration" {
  name = "Microsoft.KeyVault"
}

resource "azurerm_resource_provider_registration" "managed-identity-registration" {
  name = "Microsoft.ManagedIdentity"
}