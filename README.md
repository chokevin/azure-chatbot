# Quote Agent Bot - Azure Teams Chat Bot

A Microsoft Teams bot built with .NET 9 that provides intelligent quote management capabilities. The bot is deployed using Azure services with secure Managed Identity authentication and containerized deployment.

## 🏗️ Architecture

Our bot uses a modern, secure Azure architecture:

- **Bot Framework**: Microsoft Bot Framework with Teams integration
- **Runtime**: .NET 9 in Docker containers
- **Hosting**: Azure App Service (Linux containers)
- **Authentication**: User-Assigned Managed Identity (no secrets needed)
- **Container Registry**: Azure Container Registry (ACR)
- **Infrastructure**: Bicep templates for Infrastructure as Code
- **Deployment**: Multi-environment support (dev/prod)

## 🚀 Deployed Infrastructure

The following Azure resources are deployed:

### Core Resources
- **Managed Identity**: `kevin-test-quote-bot-prod-identity`
  - Handles authentication to Azure services
  - No secrets or passwords stored anywhere
- **Container Registry**: `kevintestquotebotprodacr.azurecr.io`
  - Stores Docker images securely
  - Managed Identity has pull permissions
- **App Service Plan**: `kevin-test-quote-bot-prod-plan` (Basic B1)
  - Linux-based for container support
- **Web App**: `kevin-test-quote-bot-prod-webapp`
  - URL: https://kevin-test-quote-bot-prod-webapp.azurewebsites.net
  - Configured for container deployment
- **Bot Service**: `kevin-test-quote-bot-prod-bot`
  - Teams channel enabled
  - Endpoint: `/api/messages`

### Security & Permissions
- **ACR Pull Role**: Managed Identity can pull container images
- **Bot Service Contributor**: Managed Identity can manage bot settings
- **Resource Group Reader**: Access for monitoring and diagnostics

## 📦 Container Deployment Options

### Manual Deployment (Current)
```bash
# 1. Build and push Docker image
az acr login --name kevintestquotebotprodacr
docker buildx build --platform linux/amd64 -t kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest .
docker push kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest

# 2. Restart web app to pull new image
az webapp restart --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3
```

### Automated Deployment Script
```bash
# Use the provided deployment script
./deploy.sh [optional-tag]

# Example with version tag
./deploy.sh v1.0.1
```

### Available Automation Options

**GitHub Actions Workflow** (`.github/workflows/deploy.yml`):
- Triggers on code changes to main branch
- Builds and deploys automatically
- Tags images with commit SHA
- Requires Azure service principal setup

**Bicep Auto-Build** (`parameters-prod-autobuild.json`):
- Builds container during infrastructure deployment
- Uses Azure deployment scripts
- Requires public repository access
- More complex but fully integrated

**Manual Script** (`deploy.sh`):
- One-command deployment
- Version tagging support
- Immediate feedback
- Good for development and testing

## 📦 Quick Deployment

### Prerequisites
- Azure CLI installed and logged in
- Docker installed
- .NET 9 SDK (for local development)

### Deploy Everything
```bash
# 1. Clone the repository
git clone <repository-url>
cd azure-chatbot

# 2. Deploy infrastructure (creates resource group and all resources)
cd infra
az group create --name kevin-test-rg3 --location "West US 2"
az deployment group create --resource-group kevin-test-rg3 --template-file main.bicep --parameters @parameters-prod.json

# 3. Build and push Docker image (manual)
cd ..
az acr login --name kevintestquotebotprodacr
docker buildx build --platform linux/amd64 -t kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest .
docker push kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest

# 4. Restart web app to pull new image
az webapp restart --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3

# OR use the deployment script for steps 3-4
./deploy.sh
```

## 🔧 Configuration

### Environment Parameters

**Development** (`parameters-dev.json`):
```json
{
  "environment": "dev",
  "location": "East US",
  "botName": "quote-agent-bot",
  "microsoftAppType": "UserAssignedMSI",
  "useContainer": false
}
```

**Production** (`parameters-prod.json`):
```json
{
  "environment": "prod",
  "location": "West US 2", 
  "botName": "kevin-test-quote-bot",
  "microsoftAppType": "UserAssignedMSI",
  "useContainer": true,
  "containerImage": "quote-agent-bot:latest"
}
```

### Key Configuration Features
- **Environment-aware**: Different settings for dev/prod
- **Container Support**: Optional containerized deployment
- **Managed Identity**: Secure authentication without secrets
- **Auto-scaling**: App Service Plan can scale based on demand

## 🏃‍♂️ Local Development

### Option 1: Direct .NET Run
```bash
cd src
dotnet restore
dotnet run
```

### Option 2: Docker Development
```bash
# Build local image
docker build -t quote-agent-bot:dev .

# Run locally
docker run -p 3978:8080 quote-agent-bot:dev
```

### Bot Framework Emulator
1. Download [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator)
2. Connect to `http://localhost:3978/api/messages`
3. Test bot interactions locally

## 📁 Project Structure

```
├── src/                           # Bot application source code
│   ├── Controllers/               # API controllers
│   ├── Models/                    # Data models
│   ├── Program.cs                 # Application entry point
│   ├── Quote.Agent.csproj         # Project file
│   └── appsettings.json          # App configuration
├── infra/                         # Infrastructure as Code
│   ├── main.bicep                # Main Bicep template
│   ├── parameters-dev.json       # Dev environment config
│   ├── parameters-prod.json      # Prod environment config
│   └── parameters-prod-autobuild.json # Prod with auto-build
├── .github/workflows/             # GitHub Actions (optional)
│   └── deploy.yml                # Automated deployment
├── TeamsApp/                      # Teams application package
│   └── appPackage/               # Teams manifest and icons
├── Dockerfile                     # Container build instructions
├── deploy.sh                      # Manual deployment script
└── README.md                     # This file
```

## 🔐 Security Features

### Managed Identity Benefits
- ✅ No secrets stored in code or config
- ✅ Automatic token rotation
- ✅ Azure RBAC integration
- ✅ Audit trail for access
- ✅ Reduced attack surface

### Container Security
- ✅ Minimal base image (Alpine Linux)
- ✅ Non-root user execution
- ✅ Private container registry
- ✅ Image scanning (via ACR)

### Network Security
- ✅ HTTPS-only communication
- ✅ Azure-managed TLS certificates
- ✅ Private networking options available

## 🚀 Deployment Environments

| Environment | Resource Group | Location | SKU | Container |
|-------------|---------------|----------|-----|-----------|
| Development | `quote-agent-dev-rg` | East US | F1 (Free) | No |
| Production | `kevin-test-rg3` | West US 2 | B1 (Basic) | Yes |

## 📊 Monitoring & Logging

- **Application Insights**: Automatic telemetry collection
- **App Service Logs**: Container and application logs
- **Bot Analytics**: Teams channel usage metrics
- **Resource Health**: Azure resource monitoring

## 🔄 CI/CD Pipeline (Optional)

Future enhancements available:
- **GitHub Actions**: Automated deployment on code changes
- **Azure DevOps**: Multi-environment promotion pipeline
- **Bicep Auto-Build**: Container building within infrastructure deployment
- **Automated Testing**: Unit and integration test pipeline
- **Security Scanning**: Container and code vulnerability scanning
- **Performance Monitoring**: Automated performance regression detection

Available automation files:
- `.github/workflows/deploy.yml` - GitHub Actions workflow
- `deploy.sh` - Manual deployment script
- `parameters-prod-autobuild.json` - Auto-build Bicep parameters

## 🆘 Troubleshooting

### Common Issues

**Bot not responding in Teams:**
1. Check web app is running: `az webapp show --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3`
2. Verify bot endpoint in Azure Bot Service
3. Check application logs: `az webapp log tail --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3`

**Container deployment issues:**
1. Verify image was pushed: `az acr repository show --name kevintestquotebotprodacr --image quote-agent-bot:latest`
2. Check managed identity permissions
3. Restart web app: `az webapp restart --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3`

### Useful Commands

```bash
# View all resources
az resource list --resource-group kevin-test-rg3 --output table

# Check web app status
az webapp show --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3 --query "state"

# View application logs
az webapp log tail --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3

# Manual deployment
./deploy.sh [version-tag]

# Update container image manually
az webapp config container set --name kevin-test-quote-bot-prod-webapp --resource-group kevin-test-rg3 --docker-custom-image-name kevintestquotebotprodacr.azurecr.io/quote-agent-bot:latest

# Check container registry images
az acr repository list --name kevintestquotebotprodacr --output table
az acr repository show-tags --name kevintestquotebotprodacr --repository quote-agent-bot --output table
```

## 📚 Additional Documentation

- [Infrastructure Details](infra/README.md) - Bicep templates and deployment
- [Managed Identity Setup](MANAGED_IDENTITY_GUIDE.md) - Authentication configuration  
- [Configuration Guide](CONFIGURATION.md) - Environment setup
- [Teams Integration](test.md) - Teams app manifest and deployment

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test locally with Bot Framework Emulator
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
