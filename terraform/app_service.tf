data "azurerm_service_plan" "hro-app-service-plan" {
  name                = var.service_plan_name
  resource_group_name = var.resource_group_name
}

resource "azurerm_windows_web_app_slot" "hro-scheduling-api-staging-slot" {
  name           = "staging"
  app_service_id = azurerm_windows_web_app.hro-scheduling-api.id

  app_settings = {
    ASPNETCORE_ENVIRONMENT    = "Staging"
    AUTHENTICATION_IDENTIFIER = var.authentication_identifier
    JWT_SECRET                = var.jwt_secret_staging
    SENTRY_DSN                = var.sentry_dsn
    DrsOptions__ApiAddress    = var.drs_api_address
    DrsOptions__Login         = var.drs_login
    DrsOptions__Password      = var.drs_password
  }

  auth_settings {
    enabled = false
  }

  site_config {}
}

resource "azurerm_windows_web_app" "hro-scheduling-api" {
  name                = var.app_service_name
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location
  service_plan_id     = data.azurerm_service_plan.hro-app-service-plan.id

  auth_settings {
    enabled = false
  }

  site_config {}
  app_settings = {
    ASPNETCORE_ENVIRONMENT    = "Production"
    AUTHENTICATION_IDENTIFIER = var.authentication_identifier
    JWT_SECRET                = var.jwt_secret_production
    SENTRY_DSN                = var.sentry_dsn
    DrsOptions__ApiAddress    = var.drs_api_address
    DrsOptions__Login         = var.drs_login
    DrsOptions__Password      = var.drs_password
  }
}
