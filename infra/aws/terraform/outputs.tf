output "ecr_repository_url" {
  description = "ECR repository URL for Dutielly web image."
  value       = aws_ecr_repository.app.repository_url
}

output "apprunner_service_url" {
  description = "Public App Runner URL."
  value       = aws_apprunner_service.web.service_url
}

output "rds_endpoint" {
  description = "Private RDS SQL Server endpoint."
  value       = aws_db_instance.sqlserver.endpoint
}

output "connection_string_secret_arn" {
  description = "Secrets Manager ARN used by App Runner for ConnectionStrings__DutiellyDb."
  value       = aws_secretsmanager_secret.db_connection.arn
}
