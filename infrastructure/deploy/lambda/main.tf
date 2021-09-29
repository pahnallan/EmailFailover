terraform {
  required_version = ">= 0.14"
  backend s3 {}
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

variable "file_name" {
  description       = "Path to the zipped package containing the lambda runtime code"
  default           = "EmailServiceLambda.zip"
}

variable "lambda_sqs_policy_file_name" {
  description       = "Name of the file which contains the policy template for allowing the lambda access to SQS"
  default           = "policies/lambda-sqs-policy.json"
}

variable "handler_name" {
  description       = "Entry point into the lambda code."
  default           = "EmailFailOverLambda::EmailFailOverLambda.Function::FunctionHandler"
}

variable "timeout" {
  description       = "Amount of time the lambda function has to run in seconds"
  default           = 30
}

variable "runtime" {
  description       = "Identifier of the function's runtime"
  default           = "dotnetcore2.1"
}

variable "memory_size" {
  description       = "Amount of memory in MB your Lambda Function can use at runtime"
  default           = 128
}

variable "spend_grid_api_key" {
  description       = "API Key to access spend grid endpoint"
  default           = ""
}

variable "snail_gun_api_key" {
  description       = "API Key to access spend grid endpoint"
  default           = ""
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
  function_name                 = lower(join("-", [var.product, "lambda", var.environment]))
  lambda_role_name              = lower(join("-", [var.product, "lambda-role", var.environment]))
  lambda_role_policy_name       = lower(join("-", [var.product, "lambda-sqs-policy", var.environment]))
  lambda_policy_attachment_name = lower(join("-", [var.product, "sqs-policy-attachment", var.environment]))

  email_sqs_arn                 = "arn:aws:sqs:${var.region}:${data.aws_caller_identity.current.account_id}:${lower(join("-", [var.product, "queue", var.environment]))}"
  tags                          = {
    "Product"     = var.product,
    "Owner"       = var.owner,
    "Environment" = var.environment
  }
}

####################################################################################################
# IAM Roles Policies for Email Service Lambda
####################################################################################################
resource "aws_iam_role" "email_service_iam_role" {
  name                     = local.lambda_role_name
  assume_role_policy        = jsonencode({
      Version = "2012-10-17"
      Statement = [
        {
          Action = "sts:AssumeRole"
          Effect = "Allow"
          Sid    = ""
          Principal = {
            Service = "lambda.amazonaws.com"
          }
        },
      ]
  })
}

resource "aws_iam_policy" "lambda_sqs_access_policy" {
  name                          = local.lambda_role_policy_name
  description                   = "Policy for allowing the email service lambda access to the SQS queue to retrieve messages."
  policy                        = templatefile ("./${var.lambda_sqs_policy_file_name}", {
    resource_arn                = local.email_sqs_arn
  })
}

resource "aws_iam_policy_attachment" "api_gateway_iam_role_sqs_policy_attachment" {
  name                          = local.lambda_policy_attachment_name
  roles                         = [aws_iam_role.email_service_iam_role.name]
  policy_arn                    = aws_iam_policy.lambda_sqs_access_policy.arn
}

resource "aws_iam_role_policy_attachment" "api_gateway_iam_role_basic_lambda_policy_attachment" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
  role       = aws_iam_role.email_service_iam_role.name
}

####################################################################################################
# Email Service Lambda
####################################################################################################
resource "aws_lambda_function" "email_service_lambda_function" {
  filename                    = var.file_name
  source_code_hash            = "TestHashToDebugGitHubActionFailure"//filebase64sha256(var.file_name)
  function_name               = local.function_name
  role                        = aws_iam_role.email_service_iam_role.arn
  handler                     = var.handler_name
  runtime                     = var.runtime
  timeout                     = var.timeout
  memory_size                 = var.memory_size

  environment {
    variables = {
      SpendGridUrlEndpoint    = "https://bw-interviews.herokuapp.com/spendgrid/send_email",
      SnailGunUrlEndpoint     = "https://bw-interviews.herokuapp.com/snailgun/emails",
      SpendGridApiKey         = var.spend_grid_api_key
      SnailGunApiKey          = var.snail_gun_api_key
      ActiveEmailProvider     = "SpendGrid",
    }
  }
}

resource "aws_lambda_event_source_mapping" "example" {
  event_source_arn = local.email_sqs_arn
  function_name    = aws_lambda_function.email_service_lambda_function.arn
}

####################################################################################################
# Email Service CloudWatch Logging
####################################################################################################
//resource "aws_cloudwatch_log_group" "yada" {
//  name                        = "Yada"
//  tags                        = local.tags
//}