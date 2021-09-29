terraform {
  required_version = ">= 0.14"
  backend "s3" { }
}

####################################################################################################
# Terraform Variables
####################################################################################################
variable "region" {
  description       = "AWS Region"
  default           = "us-west-2"
}

variable "environment" {
  description       = "Environment suffix to deploy resources to"
  default           = "dev"
}

variable "product" {
  description       = "Name of the product which these resources belong to"
  default           = "email-service"
}

variable "owner" {
  description       = "Name of who team/person who owns this product/service"
  default           = "Allan Pahn"
}

variable "openapi_spec" {
  description       = "Name of the OpenAPI spec file"
  default           = "swagger.yaml"
}

variable "sqs_policy_file_name" {
  description       = "Name of the file which contains the policy template for allowing the API Gateway access to SQS"
  default           = "policies/api-gateway-sqs-policy.json"
}

variable "stage_name" {
  description       = "Name of the stage of which the API is deployed to"
  default           = "api"
}

####################################################################################################
# Terraform Existing Data Resources Reference
####################################################################################################
data "aws_caller_identity" "current" {}

####################################################################################################
# Terraform Providers
####################################################################################################
provider "aws" {
  region            = var.region
}

####################################################################################################
# Local Variables
####################################################################################################
locals {
  api_name          = lower(join("-", [var.product, var.environment]))
  api_role_name     = lower(join("-", [var.product, "api-role", var.environment]))
  api_role_policy_name   = lower(join("-", [var.product, "sqs-policy", var.environment]))
  api_policy_attachment_name = lower(join("-", [var.product, "sqs-policy-attachment", var.environment]))
  parsed_spec       = yamldecode(file("./${var.openapi_spec}"))
  client_api_access = lower(join("-", [var.product, "client-api-access", var.environment]))

  email_sqs_arn                 = "arn:aws:sqs:${var.region}:${data.aws_caller_identity.current.account_id}:${lower(join("-", [var.product, "queue", var.environment]))}"
  email_sqs_name                  = lower(join("-", [var.product, "queue", var.environment]))
  tags                        = {
    "Product"    = var.product,
    "Owner"      = var.owner
    "Environment"= var.environment
  }
}

####################################################################################################
# IAM Roles Policies for API Gateway
####################################################################################################
resource "aws_iam_role" "iam_role" {
  name                     = local.api_role_name
  assume_role_policy        = jsonencode({
      Version = "2012-10-17"
      Statement = [
        {
          Action = "sts:AssumeRole"
          Effect = "Allow"
          Sid    = ""
          Principal = {
            Service = "apigateway.amazonaws.com"
          }
        },
      ]
  })
}

resource "aws_iam_policy" "sqs_access_policy"{
  name                   = local.api_role_policy_name
  description                   = "Policy for KMS."
  policy      = templatefile ("./${var.sqs_policy_file_name}", {
    resource_arn = local.email_sqs_arn
  })
}

resource "aws_iam_policy_attachment" "api_gateway_iam_role_sqs_policy_attachment"{
  name                          = local.api_policy_attachment_name
  roles                         = [aws_iam_role.iam_role.name]
  policy_arn                    = aws_iam_policy.sqs_access_policy.arn
}

####################################################################################################
# API Gateway
####################################################################################################
resource "aws_api_gateway_rest_api" "email_service_gateway_rest_api" {
  name                          = local.api_name
  description                   = local.parsed_spec.info.description
  body                          = replace(templatefile("./${var.openapi_spec}", {
    region                      = var.region,
    account_id                  = data.aws_caller_identity.current.account_id,
    iam_role_arn                = aws_iam_role.iam_role.arn,
    email_sqs_name               = local.email_sqs_name
  }), "/title:.*${local.parsed_spec.info.title}.*/", "title: '${local.api_name}'")
  endpoint_configuration {
    types = ["EDGE"] # TODO: switch this to regional when implementing a region based fail-over
  }
  tags                          = local.tags
}

# API Gateway Stage Deployment
resource "aws_api_gateway_deployment" "email_service_gateway_deployment" {
  rest_api_id       = aws_api_gateway_rest_api.email_service_gateway_rest_api.id
  stage_description = "Deployed at: ${timestamp()}"

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_api_gateway_stage" "email_service_gateway_stage" {
  stage_name = var.stage_name
  rest_api_id = aws_api_gateway_rest_api.email_service_gateway_rest_api.id
  deployment_id = aws_api_gateway_deployment.email_service_gateway_deployment.id
  tags = local.tags
}

# API Gateway API Key
resource "aws_api_gateway_api_key" "api_key" {
  name = local.client_api_access
}

# API Gateway Usage Plan and Plan Key
resource "aws_api_gateway_usage_plan" "usage_plan" {
  name = local.client_api_access

  api_stages {
    api_id = aws_api_gateway_rest_api.email_service_gateway_rest_api.id
    stage  = aws_api_gateway_stage.email_service_gateway_stage.stage_name
  }

  #depends_on = [aws_api_gateway_rest_api.email_service_gateway_rest_api, ]
}

resource "aws_api_gateway_usage_plan_key" "plan_key" {
  key_id        = aws_api_gateway_api_key.api_key.id
  key_type      = "API_KEY"
  usage_plan_id = aws_api_gateway_usage_plan.usage_plan.id
}