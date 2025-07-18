#!/bin/bash

# Script to create Azure AD App Registration for Bot Service
# This script creates an App Registration with the required permissions for a Bot Service

DISPLAY_NAME="${1:-Azure Bot Sample for Teams}"
DESCRIPTION="${2:-Echo bot with Teams integration built with .NET}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Check if Azure CLI is installed and user is logged in
echo -e "${YELLOW}Checking Azure CLI authentication...${NC}"

if ! command -v az &> /dev/null; then
    echo -e "${RED}Error: Azure CLI is not installed. Please install Azure CLI first.${NC}"
    exit 1
fi

if ! az account show &> /dev/null; then
    echo -e "${RED}Error: Please log in to Azure CLI first: az login${NC}"
    exit 1
fi

ACCOUNT=$(az account show --query "user.name" -o tsv)
echo -e "${GREEN}✓ Logged in to Azure as: $ACCOUNT${NC}"

# Create App Registration
echo -e "${YELLOW}Creating Azure AD App Registration...${NC}"

# Create the app registration
APP_REGISTRATION=$(az ad app create \
    --display-name "$DISPLAY_NAME" \
    --sign-in-audience "AzureADMultipleOrgs" \
    --query "{appId:appId,objectId:id}" -o json)

if [ $? -ne 0 ]; then
    echo -e "${RED}Error: Failed to create App Registration${NC}"
    exit 1
fi

APP_ID=$(echo $APP_REGISTRATION | jq -r '.appId')
OBJECT_ID=$(echo $APP_REGISTRATION | jq -r '.objectId')

echo -e "${GREEN}✓ Created App Registration with App ID: $APP_ID${NC}"

# Create a client secret
echo -e "${YELLOW}Creating client secret...${NC}"
SECRET=$(az ad app credential reset \
    --id $APP_ID \
    --years 2 \
    --query "password" -o tsv)

if [ $? -ne 0 ]; then
    echo -e "${RED}Error: Failed to create client secret${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Created client secret (expires in 2 years)${NC}"

# Set required API permissions for Bot Framework
echo -e "${YELLOW}Setting API permissions...${NC}"

# Microsoft Graph permissions for bot
az ad app permission add \
    --id $APP_ID \
    --api 00000003-0000-0000-c000-000000000000 \
    --api-permissions e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope

if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Warning: Failed to add API permissions. You may need to add them manually.${NC}"
fi

echo -e "${GREEN}✓ Added Microsoft Graph permissions${NC}"

# Output environment variables
echo ""
echo -e "${GREEN}App Registration created successfully!${NC}"
echo -e "${YELLOW}Please set the following environment variables:${NC}"
echo ""
echo -e "${CYAN}For current session (Bash):${NC}"
echo -e "${WHITE}export BOT_APP_ID='$APP_ID'${NC}"
echo -e "${WHITE}export BOT_APP_PASSWORD='$SECRET'${NC}"
echo -e "${WHITE}export BOT_APP_TYPE='MultiTenant'${NC}"
echo -e "${WHITE}export BOT_APP_TENANT_ID=''${NC}"
echo ""
echo -e "${CYAN}Or add them to your .env file:${NC}"
echo -e "${WHITE}BOT_APP_ID=$APP_ID${NC}"
echo -e "${WHITE}BOT_APP_PASSWORD=$SECRET${NC}"
echo -e "${WHITE}BOT_APP_TYPE=MultiTenant${NC}"
echo -e "${WHITE}BOT_APP_TENANT_ID=${NC}"
echo ""
echo -e "${RED}⚠️  IMPORTANT: Save the client secret securely. It cannot be retrieved again!${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "${WHITE}1. Set the environment variables above${NC}"
echo -e "${WHITE}2. Run 'azd up' to deploy your bot to Azure${NC}"
echo -e "${WHITE}3. Follow the Teams deployment guide in TEAMS-DEPLOYMENT.md${NC}"

# Optionally set environment variables for current session
echo ""
read -p "Do you want to set these environment variables for the current session? (y/n): " -r
if [[ $REPLY =~ ^[Yy]$ ]]; then
    export BOT_APP_ID="$APP_ID"
    export BOT_APP_PASSWORD="$SECRET"
    export BOT_APP_TYPE="MultiTenant"
    export BOT_APP_TENANT_ID=""
    echo -e "${GREEN}✓ Environment variables set for current session${NC}"
    echo -e "${YELLOW}Note: These will be lost when you close the terminal. Consider adding them to your shell profile.${NC}"
fi
