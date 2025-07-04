# Voting System Deployment Guide

## Prerequisites
- .NET 8.0 SDK or later
- SQL Server 2022 or later (for production)
- Azure subscription (for cloud deployment)
- Azure CLI installed
- Git

## Environment Setup

### 1. Database Setup
```bash
# Create production database
sqlcmd -S <server_name> -Q "CREATE DATABASE VotingSystem_Prod"

# Apply migrations
dotnet ef database update --environment Production
```

### 2. Environment Variables
Create a `.env` file in the project root with the following variables:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=<server>;Database=VotingSystem_Prod;User Id=<user>;Password=<password>;
ASPNETCORE_URLS=https://+:443;http://+:80
```

### 3. Azure Key Vault Setup
```bash
# Create Key Vault
az keyvault create --name voting-system-vault --resource-group <resource-group> --location <location>

# Store secrets
az keyvault secret set --vault-name voting-system-vault --name "ConnectionStrings--DefaultConnection" --value "<connection-string>"
```

## Deployment Steps

### 1. Build the Application
```bash
dotnet publish -c Release -o ./publish
```

### 2. Database Backup
```bash
# Backup database
sqlcmd -S <server_name> -Q "BACKUP DATABASE VotingSystem_Prod TO DISK = 'C:\Backups\VotingSystem_Prod_$(Get-Date -Format 'yyyyMMdd').bak'"
```

### 3. Deploy to Azure App Service
```bash
# Deploy to Azure
az webapp deployment source config-zip --resource-group <resource-group> --name <app-name> --src ./publish.zip
```

## Monitoring Setup

### 1. Application Insights
- Create Application Insights resource in Azure
- Add instrumentation key to appsettings.json
- Configure alerts for:
  - Error rate > 1%
  - Response time > 2 seconds
  - Server CPU > 80%
  - Memory usage > 80%

### 2. Health Checks
Monitor the following endpoints:
- `/health` - Application health
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

## Backup and Restore Procedures

### Database Backup
```bash
# Automated backup script
$date = Get-Date -Format "yyyyMMdd"
$backupPath = "C:\Backups\VotingSystem_Prod_$date.bak"
sqlcmd -S <server_name> -Q "BACKUP DATABASE VotingSystem_Prod TO DISK = '$backupPath'"
```

### Database Restore
```bash
# Restore from backup
sqlcmd -S <server_name> -Q "RESTORE DATABASE VotingSystem_Prod FROM DISK = 'C:\Backups\VotingSystem_Prod_20240315.bak'"
```

## Troubleshooting Guide

### Common Issues

1. **Database Connection Issues**
   - Check connection string in Azure Key Vault
   - Verify SQL Server firewall rules
   - Check network connectivity

2. **Application Startup Failures**
   - Check application logs in Azure Portal
   - Verify environment variables
   - Check database migrations

3. **Performance Issues**
   - Monitor Application Insights
   - Check database query performance
   - Verify caching configuration

## Security Checklist

- [ ] SSL/TLS certificates installed
- [ ] Firewall rules configured
- [ ] Database backups scheduled
- [ ] Security headers configured
- [ ] Rate limiting enabled
- [ ] Authentication configured
- [ ] Authorization rules set
- [ ] Audit logging enabled

## Maintenance Procedures

### Regular Maintenance Tasks
1. Database index maintenance
2. Log rotation
3. Certificate renewal
4. Security updates
5. Performance monitoring

### Update Procedures
1. Backup database
2. Deploy new version
3. Run database migrations
4. Verify health checks
5. Monitor for issues

## Contact Information

- System Administrator: [Contact Info]
- Database Administrator: [Contact Info]
- Security Team: [Contact Info]
- Support Team: [Contact Info] 