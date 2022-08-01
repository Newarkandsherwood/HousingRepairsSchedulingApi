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
    AUTHENTICATION_IDENTIFIER = var.authentication_identifier_staging
    JWT_SECRET                = var.jwt_secret_staging
    SENTRY_DSN                = var.sentry_dsn
    DrsOptions__ApiAddress    = var.drs_api_address_staging
    DrsOptions__Login         = var.drs_login_staging
    DrsOptions__Password      = var.drs_password_staging
    DrsOptions__Contract      = var.drs_contract_staging
    DrsOptions__Priority      = var.drs_priority_staging
  }

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
    identity_ids = [var.service_principal_id]
  }

  key_vault_reference_identity_id = var.service_principal_id

  app_settings = {
    ASPNETCORE_ENVIRONMENT    = "Production"
    AUTHENTICATION_IDENTIFIER = var.authentication_identifier_production
    JWT_SECRET                = var.jwt_secret_production
    SENTRY_DSN                = var.sentry_dsn
    DrsOptions__ApiAddress    = var.drs_api_address_production
    DrsOptions__Login         = var.drs_login_production
    DrsOptions__Password      = var.drs_password_production
    DrsOptions__Contract      = var.drs_contract_production
    DrsOptions__Priority      = var.drs_priority_production
  }
}
