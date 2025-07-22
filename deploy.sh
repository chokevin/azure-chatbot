#!/bin/bash

# Enhanced deployment script for Quote Agent Bot
# This script builds, pushes, and deploys the container image

set -e

# Configuration
RESOURCE_GROUP="kevin-test-rg3"
REGISTRY_NAME="kevintestquotebotprodacr"
WEB_APP_NAME="kevin-test-quote-bot-prod-webapp"
IMAGE_NAME="quote-agent-bot"
IMAGE_TAG=${1:-"latest"}

echo "🚀 Starting deployment process..."
echo "Resource Group: $RESOURCE_GROUP"
echo "Registry: $REGISTRY_NAME.azurecr.io"
echo "Web App: $WEB_APP_NAME"
echo "Image: $IMAGE_NAME:$IMAGE_TAG"
echo ""

# Check if logged into Azure
if ! az account show &> /dev/null; then
    echo "❌ Please log in to Azure CLI first: az login"
    exit 1
fi

# Login to ACR
echo "🔐 Logging into Azure Container Registry..."
az acr login --name $REGISTRY_NAME

# Build and push the image
echo "🏗️  Building Docker image..."
docker buildx build --platform linux/amd64 \
    -t $REGISTRY_NAME.azurecr.io/$IMAGE_NAME:$IMAGE_TAG \
    -t $REGISTRY_NAME.azurecr.io/$IMAGE_NAME:latest \
    --push .

# Update the web app
echo "🔄 Updating Web App container configuration..."
az webapp config container set \
    --name $WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --docker-custom-image-name $REGISTRY_NAME.azurecr.io/$IMAGE_NAME:$IMAGE_TAG

# Restart the web app
echo "♻️  Restarting Web App..."
az webapp restart \
    --name $WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP

# Verify deployment
echo "✅ Deployment completed successfully!"
echo ""
echo "🔗 Bot endpoint: https://$WEB_APP_NAME.azurewebsites.net/api/messages"
echo "📊 Monitor logs: az webapp log tail --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP"
echo ""
echo "🧪 Test with: curl -X POST -H 'Content-Type: application/json' -d '{\"type\":\"message\",\"text\":\"test\"}' https://$WEB_APP_NAME.azurewebsites.net/api/messages"
