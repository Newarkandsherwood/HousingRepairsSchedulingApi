resource "azurerm_key_vault" "hro-scheduling-api-key-vault" {
  name                        = "${var.app_service_name}-key-vault"
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
  object_id    = var.service_principal_id

  secret_permissions = [
    "Get",
  ]
}

