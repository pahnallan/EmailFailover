# EmailFailover
A repo containing an intermediate email service that provides an abstraction over additional email service providers.

## SwaggerHub OpenAPI Specifications

https://app.swaggerhub.com/apis/pahnallan/EmailServiceApi/1.0.0

*Note that any changes to the OpenAPI spec on swaggerhub is synced to this GitHub Repo. Upon saving and syncing from SwaggerHub, SwaggerHub will publish a new YAML file to the SwaggerHub branch located at [this link](https://github.com/pahnallan/EmailFailoverAPI/blob/master/infrastructure/deploy/api_gateway/swagger.yaml) . This SwaggerHub branch needs to be merged into the main branches to allow the CI/CD workflows to pick up the changes and redeploy the API based off the new OpenAPI YAML file.

## Technologies Used

1. AWS Services: API Gateway, SQS, CloudWatch, Lambda, S3
2. Terraform v0.14.6
3. GitHub Actions
4. .Net Core 2.1


## Overview

This Intermediate Email Service API is built on top of AWS API Gateway. At the moment it only supports asynchronous POST requests because the incoming request message goes through the API GateWay and gets placed into an SQS queue and returns Success to the caller. From there, an SQS event triggers a lambda and passes the SQS message containing the incoming request to it. The lambda processes the request and determines which email provider to send to based off of it's environment variable configurations ([active environment variable](https://github.com/pahnallan/EmailFailoverAPI/blob/master/infrastructure/deploy/lambda/main.tf#L156)). If for any reason the lambda can't successfully send to third party email provider after 4 attempts, the sqs message will automatically get placed into the dead letter queue.

## Infrastructure as Code and CI/CD

The deployment of the Intermediate Email API can be separated into 3 pieces to reduce the blast radius when making changes. Below are the 3 main components along with their github workflows source file, workflow view, and terraform files containing all the declared aws resources necessary for that piece.
1. Lambda 
    1. [Lambda GitHub Workflow Deployment Source File](https://github.com/pahnallan/EmailFailoverAPI/blob/master/.github/workflows/deploy_lambda.yaml)
    2. [Lambda GitHub Workflow Deployment View](https://github.com/pahnallan/EmailFailoverAPI/actions/workflows/deploy_lambda.yaml)
    3. [Lambda Terraform AWS file](https://github.com/pahnallan/EmailFailoverAPI/blob/master/infrastructure/deploy/lambda/main.tf)
    4. See Intermediate Email Service Runtime Code section for more info on the source code on the lambda.
2. SQS 
    1. [SQS GitHub Workflow Deployment Source File](https://github.com/pahnallan/EmailFailoverAPI/blob/master/.github/workflows/deploy_sqs.yaml)
    2. [SQS GitHub Workflow Deployment View](https://github.com/pahnallan/EmailFailoverAPI/actions/workflows/deploy_sqs.yaml)
    3. [SQS Terraform AWS file](https://github.com/pahnallan/EmailFailoverAPI/blob/master/infrastructure/deploy/sqs/main.tf)
4. API Gateway 
    1. [API Gateway GitHub Workflow Deployment Source File](https://github.com/pahnallan/EmailFailoverAPI/blob/master/.github/workflows/deploy_api_gateway.yaml)
    2. [API Gateway GitHub Workflow Deployment View](https://github.com/pahnallan/EmailFailoverAPI/actions/workflows/deploy_api_gateway.yaml)
    3. [API Gateway Terraform AWS file](https://github.com/pahnallan/EmailFailoverAPI/blob/master/infrastructure/deploy/api_gateway/main.tf)



## Intermediate Email Service Runtime Code

The source code for the Lambda runtime can be found [here](https://github.com/pahnallan/EmailFailoverAPI/tree/master/source/EmailFailOverLambda). The entrypoint for the Lambda calls is in the [EmailFailOverLambda project folder](https://github.com/pahnallan/EmailFailoverAPI/tree/master/source/EmailFailOverLambda/EmailFailOverLambda). The EmailFailoverLambda project references the [Ap.IntermediateEmailClient](https://github.com/pahnallan/EmailFailoverAPI/tree/master/source/EmailFailOverLambda/Ap.IntermediateEmailClient) which contains most of the email provider logic. 

## TODO's

1. Add monitoring on the DLQ to alert if an email provider is down.
2. Add monitoring on the SnailGun API to monitor if requests are failing after being enqued
3. Move API Keys to Secret Manager and have the Lambda instances grab them there rather than from the environment variables. Currently not as secure.
4. Add syncronous endpoint to the Intermediate API that does more fine grain validations on the request like validating email regex rather than solely relying on the API Gateway.
5. Add Mock Unit Test Services for Third Party Email Providers. Right now the unit tests are connecting directly to the test services previously provided.
6. Create a separate GitHub workflow for handling the building and deployment of the Lambda runtime to a package repository like Artifactory.
7. Have the Lambda deployment github workflow download the latest lambda runtime from the package repository and bundle it up when deploying the AWS Lambda. 
8. Add a develop branch to correspond to non-prod development, and have master be the production development branch. Right now it's only 1 branch for development but with the GitHub workflows already set up we can easily set up additional Intermediate Email APIs easily.
9. Add regional/availability zones fail overs. Currently the API is only deployed on us-west-2 and ideally we should deploy to multiple regional/availability zones in case a data center goes down. 
10. Move the Active Email Provider from lambda's environment variable to Parameter Store, and then create a separate job that can update that value individually. Right now we have to run the lambda terraform deploy job to get the value change out to production. Terraform should be "intelligent" enough to determine that only the active email provider has changed so that's the only piece to deploy but given my personal experience I don't always trust it as much.
11. Add a workflow that encapsulates the 3 existing workflows (sqs, lambda, api gateway) and deploys them all sequentially (useful for quickly standing up extra qa environments)
12. Add custom domain name to Route 53 to map to this API path.

## How to interface with the API 
Example curl request to the API endpoint
```
curl --location --request POST 'https://luvc7m2fsd.execute-api.us-west-2.amazonaws.com/api/email' \
--header 'x-api-key: <Insert Api Key>' \
--header 'Content-Type: application/json' \
--data-raw '{
    "from": "noreply@mybrightwheel.com",
    "from_name": "brightwheel",
    "to": "pahnallan@gmail.com",
    "to_name": "Allan Pahn",
    "subject": "Your Weekly Report",
    "body": "This email is sending to the API GateWay"
}'
```


Example JSON response from the API Endpoint
```
{
    "RequestId": "d1561127-5f86-5432-87c8-41e85d34fa79",
    "MessageId": "20ebd938-af7f-2343-80c6-eb62e7b19459"
}
```

The RequestId corresponds the Id of the entire request from start to finish. This is useful for tracing the request through all the services in AWS. 
The MessageId corresponds the Id of the SQS message generated. This is useful for tracing a sqs message in the logs. 

