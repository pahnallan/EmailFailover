terraform {
  required_version = ">= 0.14"
  backend "s3" {
    bucket = "allan-tf-state-bucket"
    key    = "email-service/sqs.tfstate"
    region = "us-west-2"
  }
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

variable "lambda_sqs_policy_file_name" {
  description       = "Name of the file which contains the policy template for allowing the lambda access to SQS"
  default           = "policies/lambda-sqs-policy.json"
}

variable "visibility_timeout_seconds" {
  description       = "Amount of time before a message is visible again in seconds"
  default           = "30"
}

variable "message_retention_seconds" {
  description       = "Amount of time before a message is automatically deleted"
  default           = "345600"
}

variable "max_message_size" {
  description       = "Max size allowed for a message"
  default           = "262144"
}

variable "delay_seconds" {
  default = "0"
}

variable "receive_wait_time_seconds" {
  default = "0"
}

variable "policy" {
  default = ""
}

variable "dlq_max_receive_count" {
  default = 4
}

variable "dlq_queue_arn" {
  default = ""
}

variable "fifo_queue" {
  default = false
}

variable "content_based_deduplication" {
  default = false
}

####################################################################################################
# Terraform Existing Data Resources Reference
####################################################################################################
data "aws_caller_identity" "current" {}

# data "aws_iam_account_alias" "current" {}

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
  queue_name                    = lower(join("-", [var.product, "queue", var.environment]))
  dlq_name                      = lower(join("-", [var.product, "dlq", var.environment]))

  email_sqs_arn                 = "arn:aws:sqs:${var.region}}:${data.aws_caller_identity.current.account_id}:${lower(join("-", [var.product, "queue", var.environment]))}"
  tags                        = {
    "Product"     = var.product,
    "Owner"       = var.owner,
    "Environment" = var.environment
  }
}

####################################################################################################
# IAM Roles Policies for Email Service Queue
####################################################################################################


####################################################################################################
# Standard SQS and Dead Letter Queues
####################################################################################################
resource "aws_sqs_queue" "email_service_queue" {
  name                              = local.queue_name
  visibility_timeout_seconds        = var.visibility_timeout_seconds
  delay_seconds                     = var.delay_seconds
  policy                            = var.policy
  redrive_policy                    = "{\"deadLetterTargetArn\":\"${aws_sqs_queue.email_service_dlq.arn}\",\"maxReceiveCount\":${var.dlq_max_receive_count}}"
  tags                              = local.tags
}


resource "aws_sqs_queue" "email_service_dlq" {
  name                              = local.dlq_name
  visibility_timeout_seconds        = var.visibility_timeout_seconds
  delay_seconds                     = var.delay_seconds
  policy                            = var.policy
  tags                              = local.tags
}

