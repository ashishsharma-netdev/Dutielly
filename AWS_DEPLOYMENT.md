# Dutielly AWS Deployment

This repo is prepared for an AWS deployment using:

- Docker container for `Dutielly.Web`
- Amazon ECR for container images
- AWS App Runner for the public web application
- Amazon RDS for SQL Server
- AWS Secrets Manager for `ConnectionStrings__DutiellyDb`
- Terraform for infrastructure
- GitHub Actions for CI/CD

## What I Need To Actually Deploy

Please provide these before I can deploy into your AWS account:

- AWS account ID
- AWS region, for example `ap-south-1`
- Deployment environment name, usually `prod`
- GitHub repository owner/name and default branch, if this will run from GitHub Actions
- An AWS IAM role ARN for GitHub Actions OIDC, stored as `AWS_ROLE_TO_ASSUME`
- Terraform state bucket name and DynamoDB lock table name, or permission to create them
- Optional custom domain name, for example `app.dutielly.com`
- Confirmation of RDS SQL Server size and budget. The default is SQL Server Express on `db.t3.small`.

## Prepared Production Target

These values are prepared for the current GitHub repository and your AWS account.

```text
AWS account ID: 389642461024
AWS region: ap-south-1
GitHub repository: ashishsharma-netdev/Dutielly
Default branch: main
Deployment environment: prod
Terraform state bucket: dutielly-terraform-state-389642461024-ap-south-1
Terraform lock table: dutielly-terraform-locks
GitHub Actions role name: DutiellyGitHubDeployRole
GitHub Actions role ARN: arn:aws:iam::389642461024:role/DutiellyGitHubDeployRole
```

Use these account-specific policy files when creating the GitHub Actions role:

- `infra/aws/github-actions-trust-policy.prod.json`
- `infra/aws/github-actions-deploy-policy.prod.json`

## One-Time AWS State Backend

Terraform state should be stored remotely so future pipeline runs update the same AWS stack.

From a machine with AWS credentials, run:

```powershell
cd infra/aws/state-backend
terraform init
terraform apply `
  -var="aws_region=ap-south-1" `
  -var="state_bucket_name=<globally-unique-bucket-name>" `
  -var="lock_table_name=dutielly-terraform-locks"
```

Save the outputs as GitHub secrets:

- `TF_STATE_BUCKET`
- `TF_STATE_LOCK_TABLE`

## GitHub Actions Secrets

Add these to the repository:

- `AWS_ROLE_TO_ASSUME`: IAM role ARN GitHub Actions can assume.
- `TF_STATE_BUCKET`: S3 bucket for Terraform state.
- `TF_STATE_LOCK_TABLE`: DynamoDB table for Terraform locking.

For this production deployment, use:

```text
AWS_ROLE_TO_ASSUME=arn:aws:iam::389642461024:role/DutiellyGitHubDeployRole
TF_STATE_BUCKET=dutielly-terraform-state-389642461024-ap-south-1
TF_STATE_LOCK_TABLE=dutielly-terraform-locks
```

Add this GitHub Actions variable if you do not want to enter the region manually:

- `AWS_REGION`: for example `ap-south-1`

For this production deployment, use:

```text
AWS_REGION=ap-south-1
```

Policy templates are included here:

- `infra/aws/github-actions-trust-policy.example.json`
- `infra/aws/github-actions-deploy-policy.example.json`

Replace the placeholders with your AWS account ID, region, state bucket, lock table, and GitHub repository name before creating the IAM role.

## First Deploy From GitHub Actions

Run the workflow:

```text
Actions -> Deploy Dutielly Web To AWS -> Run workflow
```

The workflow will:

1. Build `Dutielly.Web`.
2. Create/confirm the ECR repository.
3. Build the Docker image.
4. Push the image to ECR using the Git commit SHA tag.
5. Create/update VPC, App Runner, RDS SQL Server, IAM roles, and Secrets Manager.
6. Print the App Runner URL.

## Future Updates

After setup, future code changes are simple:

1. Push changes to `main`.
2. GitHub Actions builds a new image.
3. The image is pushed to ECR.
4. Terraform updates App Runner to the new image tag.

No manual server copy is needed.

## Database Notes

On first cloud startup, the app creates the schema and a default `DutiellyEasy` tenant if the database is empty. It also seeds:

```text
admin@dutielly.local / Dutielly@2026!
```

To load the full local presentation dataset into AWS RDS, run the SQL scripts from a machine that can reach the private RDS endpoint:

```powershell
sqlcmd -S <rds-endpoint>,1433 -U dutiellyadmin -P "<password>" -d master -b -i database\sqlserver\001_initial_schema.sql
sqlcmd -S <rds-endpoint>,1433 -U dutiellyadmin -P "<password>" -d DutiellyDb -b -i database\sqlserver\002_seed_presentation_data.sql
sqlcmd -S <rds-endpoint>,1433 -U dutiellyadmin -P "<password>" -d DutiellyDb -b -i database\sqlserver\003_defer_provider_features.sql
```

Because the RDS database is private, this usually requires VPN, a bastion host, AWS Systems Manager port forwarding, or temporarily enabled controlled access.

## Local Manual Deploy

If you do not use GitHub Actions, the same flow can be run manually:

```powershell
cd infra/aws/terraform
terraform init `
  -backend-config="bucket=<state-bucket>" `
  -backend-config="key=dutielly/prod/terraform.tfstate" `
  -backend-config="region=ap-south-1" `
  -backend-config="dynamodb_table=<lock-table>"

terraform apply -target=aws_ecr_repository.app -target=aws_ecr_lifecycle_policy.app -auto-approve

$repo = terraform output -raw ecr_repository_url
aws ecr get-login-password --region ap-south-1 | docker login --username AWS --password-stdin ($repo -replace "/.*$", "")
docker build -f ../../../Dockerfile -t "$repo:manual-1" ../../..
docker push "$repo:manual-1"

terraform apply -auto-approve -var="image_tag=manual-1"
terraform output apprunner_service_url
```

## Files Added

- `Dockerfile`
- `.dockerignore`
- `.github/workflows/aws-deploy.yml`
- `infra/aws/terraform/*`
- `infra/aws/state-backend/*`
- `infra/aws/github-actions-*.example.json`
