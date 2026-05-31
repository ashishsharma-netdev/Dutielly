data "aws_availability_zones" "available" {
  state = "available"
}

locals {
  name_prefix = "${var.app_name}-${var.environment}"
  azs         = slice(data.aws_availability_zones.available.names, 0, 2)
}

resource "aws_ecr_repository" "app" {
  name                 = "${local.name_prefix}-web"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_ecr_lifecycle_policy" "app" {
  repository = aws_ecr_repository.app.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep the last 20 images"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 20
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

resource "aws_vpc" "main" {
  cidr_block           = "10.42.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = {
    Name        = "${local.name_prefix}-vpc"
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_subnet" "private" {
  for_each = toset(local.azs)

  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(aws_vpc.main.cidr_block, 8, index(local.azs, each.value) + 10)
  availability_zone = each.value

  tags = {
    Name        = "${local.name_prefix}-private-${each.value}"
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_security_group" "apprunner" {
  name        = "${local.name_prefix}-apprunner-sg"
  description = "App Runner VPC connector egress"
  vpc_id      = aws_vpc.main.id

  egress {
    from_port   = 1433
    to_port     = 1433
    protocol    = "tcp"
    cidr_blocks = [aws_vpc.main.cidr_block]
    description = "SQL Server to RDS"
  }

  tags = {
    Name        = "${local.name_prefix}-apprunner-sg"
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_security_group" "rds" {
  name        = "${local.name_prefix}-rds-sg"
  description = "Private SQL Server access from App Runner"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 1433
    to_port         = 1433
    protocol        = "tcp"
    security_groups = [aws_security_group.apprunner.id]
    description     = "SQL Server from App Runner"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${local.name_prefix}-rds-sg"
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_db_subnet_group" "sqlserver" {
  name       = "${local.name_prefix}-sqlserver-subnets"
  subnet_ids = values(aws_subnet.private)[*].id

  tags = {
    Name        = "${local.name_prefix}-sqlserver-subnets"
    Application = var.app_name
    Environment = var.environment
  }
}

resource "random_password" "db_password" {
  length           = 24
  special          = true
  override_special = "!#$%&*()-_=+[]{}"
}

resource "aws_db_instance" "sqlserver" {
  identifier              = "${local.name_prefix}-sqlserver"
  engine                  = "sqlserver-ex"
  instance_class          = var.db_instance_class
  allocated_storage       = var.db_allocated_storage
  storage_type            = "gp2"
  storage_encrypted       = true
  username                = var.db_username
  password                = random_password.db_password.result
  port                    = 1433
  license_model           = "license-included"
  db_subnet_group_name    = aws_db_subnet_group.sqlserver.name
  vpc_security_group_ids  = [aws_security_group.rds.id]
  publicly_accessible     = false
  multi_az                = var.db_multi_az
  backup_retention_period = var.db_backup_retention_days
  deletion_protection     = var.deletion_protection
  skip_final_snapshot     = var.skip_final_snapshot
  apply_immediately       = true

  tags = {
    Name        = "${local.name_prefix}-sqlserver"
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_secretsmanager_secret" "db_connection" {
  name                    = "${local.name_prefix}/connection-strings/dutielly-db"
  recovery_window_in_days = 7

  tags = {
    Application = var.app_name
    Environment = var.environment
  }
}

resource "aws_secretsmanager_secret_version" "db_connection" {
  secret_id = aws_secretsmanager_secret.db_connection.id
  secret_string = join("", [
    "Server=", aws_db_instance.sqlserver.address, ",1433;",
    "Database=DutiellyDb;",
    "User ID=", var.db_username, ";",
    "Password=", random_password.db_password.result, ";",
    "Encrypt=True;",
    "TrustServerCertificate=True;",
    "MultipleActiveResultSets=true"
  ])
}

resource "aws_iam_role" "apprunner_ecr_access" {
  name = "${local.name_prefix}-apprunner-ecr-access"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "build.apprunner.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "apprunner_ecr_access" {
  role       = aws_iam_role.apprunner_ecr_access.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess"
}

resource "aws_iam_role" "apprunner_instance" {
  name = "${local.name_prefix}-apprunner-instance"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "tasks.apprunner.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_role_policy" "apprunner_instance_secrets" {
  name = "${local.name_prefix}-apprunner-secrets"
  role = aws_iam_role.apprunner_instance.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue"
        ]
        Resource = aws_secretsmanager_secret.db_connection.arn
      }
    ]
  })
}

resource "aws_apprunner_vpc_connector" "main" {
  vpc_connector_name = "${local.name_prefix}-connector"
  subnets            = values(aws_subnet.private)[*].id
  security_groups    = [aws_security_group.apprunner.id]
}

resource "aws_apprunner_service" "web" {
  service_name = "${local.name_prefix}-web"

  source_configuration {
    authentication_configuration {
      access_role_arn = aws_iam_role.apprunner_ecr_access.arn
    }

    auto_deployments_enabled = false

    image_repository {
      image_identifier      = "${aws_ecr_repository.app.repository_url}:${var.image_tag}"
      image_repository_type = "ECR"

      image_configuration {
        port = "8080"

        runtime_environment_variables = {
          ASPNETCORE_ENVIRONMENT = "Production"
          ASPNETCORE_URLS        = "http://+:8080"
        }

        runtime_environment_secrets = {
          "ConnectionStrings__DutiellyDb" = aws_secretsmanager_secret.db_connection.arn
        }
      }
    }
  }

  instance_configuration {
    cpu               = var.apprunner_cpu
    memory            = var.apprunner_memory
    instance_role_arn = aws_iam_role.apprunner_instance.arn
  }

  network_configuration {
    egress_configuration {
      egress_type       = "VPC"
      vpc_connector_arn = aws_apprunner_vpc_connector.main.arn
    }
  }

  health_check_configuration {
    protocol            = "HTTP"
    path                = "/login"
    interval            = 10
    timeout             = 5
    healthy_threshold   = 1
    unhealthy_threshold = 5
  }

  tags = {
    Application = var.app_name
    Environment = var.environment
  }

  depends_on = [
    aws_iam_role_policy_attachment.apprunner_ecr_access,
    aws_iam_role_policy.apprunner_instance_secrets,
    aws_secretsmanager_secret_version.db_connection
  ]
}
