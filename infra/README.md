# Azure Infrastructure for Quote Agent Bot

This directory contains Bicep templates for deploying the Quote Agent Teams Bot to Azure using Infrastructure as Code principles.

## 🏗️ Infrastructure Components

Our Bicep template (`main.bicep`) creates a complete, secure Azure infrastructure:

### Core Resources
- **User-Assigned Managed Identity** - Secure authentication without secrets
- **Azure Container Registry (ACR)** - Private Docker image storage
- **App Service Plan** - Linux-based hosting for containers
- **Web App** - Containerized bot application hosting
- **Bot Service** - Azure Bot Framework service with Teams integration

### Security & Permissions
- **ACR Pull Role Assignment** - Managed Identity can pull container images
- **Bot Service Contributor Role** - Managed Identity can manage bot settings  
- **Resource Group Reader Role** - Access for monitoring and diagnostics

## 📁 Files Structure

```
infra/
├── main.bicep                 # Complete infrastructure template
├── parameters-dev.json        # Development environment settings
├── parameters-prod.json       # Production environment settings
└── README.md                  # This documentation
```

## 🚀 Quick Deployment

### Prerequisites

- **Azure CLI** installed and authenticated
- **Docker** for building container images
- **Subscription permissions** to create resources and role assignments

### Deploy Everything

```bash
# 1. Create resource group
az group create --name kevin-test-rg3 --location "West US 2"

# 2. Deploy infrastructure 
az deployment group create \
  --resource-group kevin-test-rg3 \
  --template-file main.bicep \
  --parameters @parameters-prod.json

# 3. Build and push container
az acr login --name kevintestquotebotprodacr
docker buildx build --platform linux/amd64 \
  -t kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest .
docker push kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest

# 4. Restart web app
az webapp restart \
  --name kevin-test-quote-bot-prod-webapp \
  --resource-group kevin-test-rg3
```

## ⚙️ Configuration Parameters

### Development Environment (`parameters-dev.json`)
```json
{
  "environment": { "value": "dev" },
  "location": { "value": "East US" },
  "botName": { "value": "quote-agent-bot" },
  "microsoftAppTenantId": { "value": "YOUR_TENANT_ID" },
  "microsoftAppType": { "value": "UserAssignedMSI" },
  "useContainer": { "value": false }
}
```

### Production Environment (`parameters-prod.json`)  
```json
{
  "environment": { "value": "prod" },
  "location": { "value": "West US 2" },
  "botName": { "value": "kevin-test-quote-bot" },
  "microsoftAppTenantId": { "value": "72f988bf-86f1-41af-91ab-2d7cd011db47" },
  "microsoftAppType": { "value": "UserAssignedMSI" },
  "useContainer": { "value": true },
  "containerImage": { "value": "quote-agent-bot:latest" }
}
```

### Key Parameters Explained

| Parameter | Description | Dev Value | Prod Value |
|-----------|-------------|-----------|------------|
| `environment` | Environment name | `dev` | `prod` |
| `location` | Azure region | `East US` | `West US 2` |
| `useContainer` | Container deployment | `false` | `true` |
| `appServicePlanSku` | App Service tier | `F1` (Free) | `B1` (Basic) |
| `microsoftAppType` | Auth method | `UserAssignedMSI` | `UserAssignedMSI` |

## 🔧 Template Features

### Conditional Resources
- **Container Registry**: Only created when `useContainer` is true
- **Role Assignments**: Automatically configured based on deployment type
- **Linux vs Windows**: App Service Plan adapts to container needs

### Environment-Aware Configuration
- **SKU Selection**: Free tier for dev, Basic+ for production
- **Resource Naming**: Environment prefix for easy identification
- **Security Settings**: Production-ready security by default

### Managed Identity Integration
- **No Secrets Required**: All authentication uses managed identity
- **Automatic Permissions**: Role assignments created automatically
- **Principle of Least Privilege**: Minimal required permissions only

## 📊 Deployed Resources

After successful deployment to `kevin-test-rg3`:

| Resource | Name | Type | Purpose |
|----------|------|------|---------|
| Identity | `kevin-test-quote-bot-prod-identity` | Managed Identity | Authentication |
| Registry | `kevintestquotebotprodacr` | Container Registry | Image storage |
| Plan | `kevin-test-quote-bot-prod-plan` | App Service Plan | Hosting |
| WebApp | `kevin-test-quote-bot-prod-webapp` | Web App | Bot runtime |
| Bot | `kevin-test-quote-bot-prod-bot` | Bot Service | Teams integration |

### Important Outputs
```json
{
  "webAppUrl": "https://kevin-test-quote-bot-prod-webapp.azurewebsites.net",
  "containerRegistryLoginServer": "kevintestquotebotprodacr.azurecr.io",
  "managedIdentityClientId": "7a3c24f6-36bd-4b24-91fe-aefb3fdbf8ac",
  "botServiceName": "kevin-test-quote-bot-prod-bot"
}
```

## 🔒 Security Architecture

### Managed Identity Benefits
- ✅ **No stored secrets** - Identity handled by Azure
- ✅ **Automatic token rotation** - Built-in security
- ✅ **Fine-grained permissions** - Role-based access control
- ✅ **Audit trail** - All access logged in Azure

### Role Assignments
```bicep
// ACR Pull - Container image access
roleDefinitionId: '7f951dda-4ed3-4680-a7ca-43fe172d538d'

// Azure Bot Service Contributor - Bot management
roleDefinitionId: '9fc6112f-f48e-4e27-8b09-72a5c94e4ae9'

// Reader - Resource group monitoring
roleDefinitionId: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
```

## 🚀 Container Deployment

### Docker Image Requirements
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine`
- **Platform**: `linux/amd64` (required for Azure Web Apps)
- **Port**: Application must listen on port `8080`
- **Health Check**: Endpoint `/health` recommended

### Image Management
```bash
# Build for correct architecture
docker buildx build --platform linux/amd64 -t quote-agent-bot:latest .

# Tag for ACR
docker tag quote-agent-bot:latest kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest

# Push to registry
docker push kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest
```

## � Monitoring & Troubleshooting

### Health Checks
```bash
# Check all resources
az resource list --resource-group kevin-test-rg3 --output table

# Web app status
az webapp show --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3 --query "state"

# Container logs
az webapp log tail --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3
```

### Common Issues

**Deployment Failures:**
1. Check role assignment permissions
2. Verify parameter values
3. Review deployment operation details:
   ```bash
   az deployment operation group list --resource-group kevin-test-rg3 --name main
   ```

**Container Issues:**
1. Verify image exists in ACR
2. Check managed identity ACR permissions
3. Validate container configuration:
   ```bash
   az webapp config show --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3
   ```

## 🌍 Multi-Environment Strategy

### Environment Isolation
- **Separate Resource Groups** per environment
- **Different Regions** for production resilience  
- **Tiered SKUs** based on environment needs
- **Environment-specific Configuration** via parameters

### Deployment Pipeline (Recommended)
```
Development → Staging → Production
     ↓            ↓         ↓
   East US   →  Central US → West US 2
   Free Tier   Basic Tier   Standard Tier
```

## 🔄 Updates & Maintenance

### Infrastructure Updates
```bash
# Update infrastructure
az deployment group create \
  --resource-group kevin-test-rg3 \
  --template-file main.bicep \
  --parameters @parameters-prod.json \
  --name "update-$(date +%Y%m%d-%H%M%S)"
```

### Application Updates
```bash
# Update container image
docker build -t kevintestquotebotprodacr.azurecr.io/quote-agent-bot:v1.1 .
docker push kevintestquotebotprodacr.azurecr.io/quote-agent-bot:v1.1

# Update web app configuration
az webapp config container set \
  --name kevin-test-quote-bot-prod-webapp \
  --resource-group kevin-test-rg3 \
  --docker-custom-image-name kevintestquotebotprodacr.azurecr.io/quote-agent-bot:v1.1
```

## � Best Practices

1. **Use Parameter Files** for environment-specific configurations
2. **Tag Resources** consistently for cost management and organization
3. **Enable Monitoring** with Application Insights and Log Analytics
4. **Implement CI/CD** for automated deployments
5. **Regular Security Reviews** of role assignments and permissions
6. **Backup Strategy** for critical configuration and data
7. **Cost Optimization** through appropriate SKU selection

## 🆘 Support & Documentation

- **Azure Bicep Documentation**: https://docs.microsoft.com/azure/azure-resource-manager/bicep/
- **Bot Framework Documentation**: https://docs.microsoft.com/azure/bot-service/
- **Managed Identity Guide**: https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/
