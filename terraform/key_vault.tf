resource "azurerm_key_vault" "hro-scheduling-api-key-vault" {
  name                       = "hro-sched-api-key-vault"
  location                   = var.resource_group_location
  resource_group_name        = var.resource_group_name
  tenant_id                  = var.azure_ad_tenant_id
  soft_delete_retention_days = 7
  purge_protection_enabled   = false
  sku_name                   = "standard"
}

resource "azurerm_key_vault_access_policy" "hro-scheduling-api-key-vault-access-policy" {
  key_vault_id = sensitive(azurerm_key_vault.hro-scheduling-api-key-vault.id)
  tenant_id    = var.azure_ad_tenant_id
  object_id    = sensitive(azurerm_user_assigned_identity.hro-scheduling-api-vault-access-identity.principal_id)

  secret_permissions = [
    "Get",
  ]
}
#=================================
#====== Shared in both environments
resource "azurerm_key_vault_secret" "sentry-dsn" {
    name         = "sentry-dsn"
    value        = var.sentry_dsn
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}


#=================================
#====== Staging secrets
resource "azurerm_key_vault_secret" "authentication-identifier-staging" {
    name         = "authentication-identifier-staging"
    value        = var.authentication_identifier_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "jwt-secret-staging" {
    name         = "jwt-secret-staging"
    value        = var.jwt_secret_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-api-address-staging" {
    name         = "drs-api-address-staging"
    value        = var.drs_api_address_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-login-staging" {
    name         = "drs-login-staging"
    value        = var.drs_login_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-password-staging" {
    name         = "drs-password-staging"
    value        = var.drs_password_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-contract-staging" {
    name         = "drs-contract-staging"
    value        = var.drs_contract_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-priority-staging" {
    name         = "drs-priority-staging"
    value        = var.drs_priority_staging
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}
#=================================
#====== Production Secrets

resource "azurerm_key_vault_secret" "authentication-identifier-production" {
    name         = "authentication-identifier-production"
    value        = var.authentication_identifier_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "jwt-secret-production" {
    name         = "jwt-secret-production"
    value        = var.jwt_secret_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-api-address-production" {
    name         = "drs-api-address-production"
    value        = var.drs_api_address_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-login-production" {
    name         = "drs-login-production"
    value        = var.drs_login_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-password-production" {
    name         = "drs-password-production"
    value        = var.drs_password_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-contract-production" {
    name         = "drs-contract-production"
    value        = var.drs_contract_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}

resource "azurerm_key_vault_secret" "drs-priority-production" {
    name         = "drs-priority-production"
    value        = var.drs_priority_production
    key_vault_id = azurerm_key_vault.hro-scheduling-api-key-vault.id
}