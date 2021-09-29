#!/bin/sh

# output commands to stdout
set -x
# exit the script on a failed command
set -e

TF_LOG=DEBUG
cd ./$1

# Output Terraform Version
terraform -v

# Run Terraform Init
terraform init -backend-config="bucket=allan-tf-state-bucket" -backend-config="region=us-west-2" -backend-config="key=email-service/$1.tfstate" -backend=true -force-copy -get=true -input=false

# Run Terraform Plan
terraform plan

# Run Terraform Apply
terraform apply -auto-approve