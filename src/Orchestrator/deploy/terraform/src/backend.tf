resource "aws_ebs_volume" "backend" {
  availability_zone = "${data.aws_region.current.name}a"
  size              = 10

  tags = merge(
    local.common_tags,
    tomap({ "Name" = "${local.prefix}-backend" })
  )
}

data "archive_file" "backend_source" {
  count       = var.app_version
  type        = "zip"
  output_path = "./backend_source_${count.index + 1}.zip"

  source {
    content = templatefile(
      "./eb/docker-compose.tftpl",
      {
        dummy_image_uri = "chekhenkho/truquest_dummy:${count.index + 1}"
      }
    )
    filename = "docker-compose.yml"
  }

  source {
    content  = file("./eb/commands.config")
    filename = ".ebextensions/commands.config"
  }

  source {
    content = templatefile(
      "./eb/mount-volume.tftpl",
      {
        mount_dir = "/backend"
        volume_id = aws_ebs_volume.backend.id
        region    = data.aws_region.current.name
      }
    )
    filename = ".platform/hooks/predeploy/mount-volume.sh"
  }
}

resource "aws_s3_bucket" "backend_source" {
  bucket        = "${local.prefix}-backend-source"
  force_destroy = true

  tags = local.common_tags
}

resource "aws_s3_object" "backend_source" {
  count  = var.app_version
  bucket = aws_s3_bucket.backend_source.id
  key    = "backend_source_${count.index + 1}.zip"
  source = "./backend_source_${count.index + 1}.zip"
  etag   = data.archive_file.backend_source[count.index].output_md5
}

resource "aws_iam_role" "backend" {
  name               = "${local.prefix}-backend"
  assume_role_policy = file("./eb/policies/ec2-assume-role-policy.json")

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "backend" {
  role       = aws_iam_role.backend.name
  policy_arn = "arn:aws:iam::aws:policy/AWSElasticBeanstalkWebTier"
}

resource "aws_iam_policy" "backend_volume_management" {
  name   = "${local.prefix}-backend-volume-management"
  policy = file("./eb/policies/backend-volume-management.json")
}

resource "aws_iam_role_policy_attachment" "backend_volume_management" {
  role       = aws_iam_role.backend.name
  policy_arn = aws_iam_policy.backend_volume_management.arn
}

resource "aws_iam_instance_profile" "backend" {
  name = "${local.prefix}-backend"
  role = aws_iam_role.backend.name
}

resource "aws_iam_role" "eb" {
  name               = "${local.prefix}-eb"
  assume_role_policy = file("./eb/policies/eb-assume-role-policy.json")

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "eb_updates" {
  role       = aws_iam_role.eb.name
  policy_arn = "arn:aws:iam::aws:policy/AWSElasticBeanstalkManagedUpdatesCustomerRolePolicy"
}

resource "aws_iam_role_policy_attachment" "eb_health" {
  role       = aws_iam_role.eb.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSElasticBeanstalkEnhancedHealth"
}

resource "aws_security_group" "backend" {
  name        = "${local.prefix}-backend"
  description = "Allows SSH, HTTP, and IPFS gateway access from anywhere"
  vpc_id      = aws_vpc.main.id

  ingress {
    protocol    = "tcp"
    from_port   = 22
    to_port     = 22
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 80
    to_port     = 80
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 8080
    to_port     = 8080
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    protocol         = "-1"
    from_port        = 0
    to_port          = 0
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }

  tags = local.common_tags
}

resource "aws_elastic_beanstalk_application" "backend" {
  name = "${local.prefix}-backend"
  tags = local.common_tags
}

resource "aws_elastic_beanstalk_application_version" "backend" {
  count       = var.app_version
  name        = "${local.prefix}-backend-version-${count.index + 1}"
  application = aws_elastic_beanstalk_application.backend.name
  bucket      = aws_s3_bucket.backend_source.id
  key         = aws_s3_object.backend_source[count.index].id

  tags = local.common_tags
}

resource "aws_elastic_beanstalk_environment" "backend_staging" {
  depends_on          = [docker_container.migrator]
  name                = "${local.prefix}-backend"
  application         = aws_elastic_beanstalk_application.backend.name
  tier                = "WebServer"
  solution_stack_name = "64bit Amazon Linux 2 v3.5.8 running Docker"
  version_label       = aws_elastic_beanstalk_application_version.backend[var.app_version - 1].name

  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = aws_vpc.main.id
  }

  setting {
    namespace = "aws:ec2:vpc"
    name      = "Subnets"
    value     = aws_subnet.public_a.id
  }

  #   setting { # Not supported for VPC environments
  #     namespace = "aws:autoscaling:asg"
  #     name      = "Custom Availability Zones"
  #     value     = "${data.aws_region.current.name}a"
  #   }

  setting {
    namespace = "aws:ec2:instances"
    name      = "InstanceTypes"
    value     = "t3.micro"
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "IamInstanceProfile"
    value     = aws_iam_instance_profile.backend.name
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "SecurityGroups"
    value     = aws_security_group.backend.id
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "EC2KeyName"
    value     = "${local.prefix}-backend"
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "StreamLogs"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "DeleteOnTerminate"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "EnvironmentType"
    value     = "SingleInstance"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "ServiceRole"
    value     = aws_iam_role.eb.name
  }

  tags = local.common_tags
}