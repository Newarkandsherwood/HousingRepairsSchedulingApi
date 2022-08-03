resource "azurerm_key_vault" "hro-scheduling-api-key-vault" {
  name                        = "hro-sched-api-key-vault"
  location                    = var.resource_group_location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = true
  tenant_id                   = var.azure_ad_tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  sku_name                    = "standard"
}

resource "azurerm_key_vault_access_policy" "hro-scheduling-api-key-vault-access-policy" {
  key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
  tenant_id    = var.azure_ad_tenant_id
  object_id    = azurerm_user_assigned_identity.hro-scheduling-api-vault-access-identity.id

  secret_permissions = [
    "Get",
  ]
}

