variable "aws_region" {
  description = "AWS region for Dutielly."
  type        = string
  default     = "ap-south-1"
}

variable "app_name" {
  description = "Application name prefix."
  type        = string
  default     = "dutielly"
}

variable "environment" {
  description = "Deployment environment name."
  type        = string
  default     = "prod"
}

variable "image_tag" {
  description = "Container image tag already pushed to ECR."
  type        = string
  default     = "bootstrap"
}

variable "apprunner_cpu" {
  description = "App Runner CPU units."
  type        = string
  default     = "1024"
}

variable "apprunner_memory" {
  description = "App Runner memory."
  type        = string
  default     = "2048"
}

variable "db_username" {
  description = "RDS SQL Server master username."
  type        = string
  default     = "dutiellyadmin"
}

variable "db_instance_class" {
  description = "RDS SQL Server instance class."
  type        = string
  default     = "db.t3.small"
}

variable "db_allocated_storage" {
  description = "RDS allocated storage in GB."
  type        = number
  default     = 20
}

variable "db_backup_retention_days" {
  description = "RDS backup retention period."
  type        = number
  default     = 7
}

variable "db_multi_az" {
  description = "Whether to run RDS SQL Server in Multi-AZ mode."
  type        = bool
  default     = false
}

variable "deletion_protection" {
  description = "Protect the RDS database from accidental deletion."
  type        = bool
  default     = true
}

variable "skip_final_snapshot" {
  description = "Skip final RDS snapshot on destroy. Keep false for real production."
  type        = bool
  default     = true
}
