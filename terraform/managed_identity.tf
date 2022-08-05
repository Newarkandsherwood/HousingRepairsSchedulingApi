resource "azurerm_user_assigned_identity" "hro-scheduling-api-vault-access-identity" {
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location

  name = "hro-scheduling-api-vault-access-identity"
}

resource "azurerm_resource_provider_registration" "managed-identity-registration" {
  name = "Microsoft.ManagedIdentity"
}
