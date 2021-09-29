terraform {
  required_version = ">= 0.14"
}

####################################################################################################
# Terraform Variables
####################################################################################################

variable "region" {
  description       = "AWS Region"
  type              = "string"
  default           = "us-west-2"
}

variable "environment" {
  description       = "Environment suffix to deploy resources to"
  type              = "string"
}

variable "product" {
  description       = "Name of the product which these resources belong to"
  type              = "string"
  default           = "email-service"
}

variable "owner" {
  description       = "Name of who team/person who owns this product/service"
  type              = "string"
  default           = "Allan Pahn"
}

variable "openapi_spec" {
  description       = "Name of the OpenAPI spec file"
  type              = "string"
}

variable "sqs_policy_file_name" {
  description       = "Name of the file which contains the policy template for allowing the API Gateway access to SQS"
  type              = "string"
  default           = "policies/api-gateway-sqs-policy.json"
}

####################################################################################################
# Terraform Existing Data Resources Reference
####################################################################################################
data "aws_caller_identity" "current" {}

data "aws_iam_account_alias" "current" {}

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

  email_sqs_arn                 = "arn:aws:sqs:${var.region}}:${data.aws_caller_identity.current.account_id}:${lower(join("-", [var.product, "queue", var.environment]))}"
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
  type                     = "Amazon API Gateway"
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
  policy_name                   = local.api_role_policy_name
  description                   = "Policy for KMS."
  policy      = templatefile ("./${var.sqs_policy_file_name}", {
    resource_arn = local.email_sqs_arn
  })
}

resource "aws_iam_policy_attachment" "api_gateway_iam_role_sqs_policy_attachment"{
  name                          = local.api_policy_attachment_name
  role                          = aws_iam_role.iam_role.name
  policy_arn                    = aws_iam_policy.sqs_access_policy.arn
}

####################################################################################################
# API Gateway with Logging
####################################################################################################
resource "aws_api_gateway_rest_api" "email_service_gateway_rest_api" {
  name                          = local.api_name
  description                   = local.parsed_spec.info.description
  body                          = replace(templatefile("./${var.openapi_spec}", {
    region                      = var.region,
    account_id                  = data.aws_caller_identity.current.account_id,
    iam_role_arn                = aws_iam_role.iam_role.arn,
    email_sqs_arn               = local.email_sqs_arn
  }), "/title:.*${local.parsed_spec.info.title}.*/", "title: '${local.api_name}'")
  endpoint_configuration {
    types = ["EDGE"] # TODO: switch this to regional when implementing a region based fail-over
  }
  tags                          = local.tags
}