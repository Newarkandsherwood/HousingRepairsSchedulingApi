data "azurerm_service_plan" "hro-app-service-plan" {
  name                = var.service_plan_name
  resource_group_name = var.resource_group_name
}

resource "azurerm_windows_web_app_slot" "hro-scheduling-api-staging-slot" {
  name           = "staging"
  app_service_id = azurerm_windows_web_app.hro-scheduling-api.id
  https_only     = true

  app_settings = {
    ASPNETCORE_ENVIRONMENT    = "Staging"
    AUTHENTICATION_IDENTIFIER = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.authentication-identifier-staging.id})"
    JWT_SECRET                = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.jwt-secret-staging.id})"
    SENTRY_DSN                = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.sentry-dsn.id})"
    DrsOptions__ApiAddress    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-api-address-staging.id})"
    DrsOptions__Login         = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-login-staging.id})"
    DrsOptions__Password      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-password-staging.id})"
    DrsOptions__Contract      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-contract-staging.id})"
    DrsOptions__Priority      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-priority-staging.id})"
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.hro-scheduling-api-vault-access-identity.id]
  }

  key_vault_reference_identity_id = azurerm_user_assigned_identity.hro-scheduling-api-vault-access-identity.id

  auth_settings {
    enabled = false
  }

  site_config {
    health_check_path = "/health"
  }
}

resource "azurerm_windows_web_app" "hro-scheduling-api" {
  name                = var.app_service_name
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location
  service_plan_id     = data.azurerm_service_plan.hro-app-service-plan.id
  https_only          = true

  auth_settings {
    enabled = false
  }

  site_config {
    health_check_path = "/health"
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.hro-scheduling-api-vault-access-identity.id]
  }

  key_vault_reference_identity_id = azurerm_user_assigned_identity.hro-scheduling-api-vault-access-identity.id

  app_settings = {
    ASPNETCORE_ENVIRONMENT    = "Production"
    AUTHENTICATION_IDENTIFIER = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.authentication-identifier-production.id})"
    JWT_SECRET                = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.jwt-secret-production.id})"
    SENTRY_DSN                = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.sentry-dsn.id})"
    DrsOptions__ApiAddress    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-api-address-production.id})"
    DrsOptions__Login         = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-login-production.id})"
    DrsOptions__Password      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-password-production.id})"
    DrsOptions__Contract      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-contract-production.id})"
    DrsOptions__Priority      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.drs-priority-production.id})"
  }
}
