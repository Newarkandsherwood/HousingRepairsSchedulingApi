variable "service_plan_name" {
  type = string
}
variable "resource_group_name" {
  type = string
}
variable "resource_group_location" {
  type = string
}
variable "app_service_name" {
  type = string
}
variable "authentication_identifier_production" {
  default   = ""
  sensitive = true
}
variable "authentication_identifier_staging" {
  default   = ""
  sensitive = true
}
variable "jwt_secret_production" {
  type      = string
  sensitive = true
}
variable "jwt_secret_staging" {
  type      = string
  sensitive = true
}
variable "sentry_dsn" {
  type = string
}
variable "drs_api_address_production" {
  default = ""
}
variable "drs_login_production" {
  default   = ""
  sensitive = true
}
variable "drs_password_production" {
  default   = ""
  sensitive = true
}
variable "drs_contract_production" {
  default = ""
}
variable "drs_priority_production" {
  default = ""
}
variable "drs_api_address_staging" {
  default = ""
}
variable "drs_login_staging" {
  default   = ""
  sensitive = true
}
variable "drs_password_staging" {
  default   = ""
  sensitive = true
}
variable "drs_contract_staging" {
  default = ""
}
variable "drs_priority_staging" {
  default = ""
}
variable "azure_ad_tenant_id" {
  type      = string
  sensitive = true
}
