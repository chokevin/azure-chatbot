@description('The environment name (dev, staging, prod)')
param environment string

@description('The Azure region to deploy resources')
param location string = resourceGroup().location

@description('The name of the bot service')
param botName string

@description('The Microsoft App ID (Client ID) for the bot')
param microsoftAppId string = ''

@description('The Microsoft App Tenant ID')
param microsoftAppTenantId string

@description('The Microsoft App Type (UserAssignedMSI, MultiTenant, SingleTenant)')
@allowed(['UserAssignedMSI', 'MultiTenant', 'SingleTenant'])
param microsoftAppType string = 'UserAssignedMSI'

@description('Whether to use container deployment')
param useContainer bool = false

@description('The container image name and tag')
param containerImage string = 'quote-agent-bot:latest'

@description('Whether to automatically build and push the Docker image')
param autoBuildContainer bool = false

@description('The GitHub repository URL for source code (needed for auto-build)')
param sourceRepoUrl string = ''

@description('The SKU for the App Service Plan')
@allowed(['F1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2'])
param appServicePlanSku string = environment == 'prod' ? 'B1' : 'F1'

// Variables
var resourcePrefix = '${botName}-${environment}'
var managedIdentityName = '${resourcePrefix}-identity'
var containerRegistryName = replace('${resourcePrefix}acr', '-', '')
var appServicePlanName = '${resourcePrefix}-plan'
var webAppName = '${resourcePrefix}-webapp'
var botServiceName = '${resourcePrefix}-bot'
var containerRegistryLoginServer = useContainer ? '${containerRegistryName}.azurecr.io' : ''

// Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: {
    Environment: environment
    Application: 'Quote Agent Bot'
  }
}

// Container Registry (only if using containers)
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = if (useContainer) {
  name: containerRegistryName
  location: location
  sku: {
    name: environment == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
  tags: {
    Environment: environment
    Application: 'Quote Agent Bot'
  }
}

// Grant ACR Pull permissions to Managed Identity
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (useContainer) {
  name: guid(containerRegistry.id, managedIdentity.id, 'AcrPull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull role
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Resource Group Reader permissions to Managed Identity (for monitoring and diagnostics)
resource resourceGroupReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, managedIdentity.id, 'Reader')
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7') // Reader role
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Container Build and Push Script (optional)
resource containerBuildScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = if (useContainer && autoBuildContainer) {
  name: '${resourcePrefix}-build-container'
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    azCliVersion: '2.50.0'
    timeout: 'PT30M'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'REGISTRY_NAME'
        value: containerRegistryName
      }
      {
        name: 'IMAGE_NAME'
        value: containerImage
      }
      {
        name: 'SOURCE_REPO'
        value: sourceRepoUrl
      }
    ]
    scriptContent: '''
      set -e
      echo "Starting container build process..."
      
      # Login to ACR using managed identity
      az acr login --name $REGISTRY_NAME
      
      # Clone the repository if URL provided
      if [ ! -z "$SOURCE_REPO" ]; then
        echo "Cloning repository: $SOURCE_REPO"
        git clone $SOURCE_REPO /tmp/source
        cd /tmp/source
      else
        echo "No source repo provided, assuming source is available"
      fi
      
      # Build the Docker image with correct architecture
      echo "Building Docker image: $REGISTRY_NAME.azurecr.io/$IMAGE_NAME"
      docker buildx create --use --name multiarch || true
      docker buildx build --platform linux/amd64 \
        -t $REGISTRY_NAME.azurecr.io/$IMAGE_NAME \
        --push .
      
      echo "Container build and push completed successfully"
    '''
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

// Grant ACR Push permissions to Managed Identity (for build script)
resource acrPushRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (useContainer && autoBuildContainer) {
  name: guid(containerRegistry.id, managedIdentity.id, 'AcrPush')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '8311e382-0749-4cb8-b61a-304f252e45ec') // AcrPush role
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    tier: appServicePlanSku == 'F1' ? 'Free' : (startsWith(appServicePlanSku, 'B') ? 'Basic' : (startsWith(appServicePlanSku, 'S') ? 'Standard' : 'PremiumV2'))
  }
  kind: useContainer ? 'linux' : 'app'
  properties: {
    reserved: useContainer // true for Linux, false for Windows
  }
  tags: {
    Environment: environment
    Application: 'Quote Agent Bot'
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  kind: useContainer ? 'app,linux,container' : 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: useContainer ? {
      linuxFxVersion: 'DOCKER|${containerRegistryLoginServer}/${containerImage}'
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: managedIdentity.properties.clientId
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'MicrosoftAppType'
          value: microsoftAppType
        }
        {
          name: 'MicrosoftAppId'
          value: microsoftAppId != '' ? microsoftAppId : managedIdentity.properties.clientId
        }
        {
          name: 'MicrosoftAppTenantId'
          value: microsoftAppTenantId
        }
      ]
    } : {
      netFrameworkVersion: 'v9.0'
      appSettings: [
        {
          name: 'MicrosoftAppType'
          value: microsoftAppType
        }
        {
          name: 'MicrosoftAppId'
          value: microsoftAppId != '' ? microsoftAppId : managedIdentity.properties.clientId
        }
        {
          name: 'MicrosoftAppTenantId'
          value: microsoftAppTenantId
        }
      ]
    }
    httpsOnly: true
  }
  tags: {
    Environment: environment
    Application: 'Quote Agent Bot'
  }
  dependsOn: useContainer ? (autoBuildContainer ? [acrPullRoleAssignment, containerBuildScript] : [acrPullRoleAssignment]) : []
}

// Bot Service
resource botService 'Microsoft.BotService/botServices@2022-09-15' = {
  name: botServiceName
  location: 'global'
  sku: {
    name: environment == 'prod' ? 'S1' : 'F0'
  }
  kind: 'azurebot'
  properties: {
    displayName: '${botName} (${environment})'
    endpoint: 'https://${webApp.properties.defaultHostName}/api/messages'
    msaAppType: microsoftAppType
    msaAppId: microsoftAppId != '' ? microsoftAppId : managedIdentity.properties.clientId
    msaAppTenantId: microsoftAppTenantId
    msaAppMSIResourceId: microsoftAppType == 'UserAssignedMSI' ? managedIdentity.id : null
    schemaTransformationVersion: '1.3'
  }
  tags: {
    Environment: environment
    Application: 'Quote Agent Bot'
  }
}

// Bot Service Teams Channel
resource teamsChannel 'Microsoft.BotService/botServices/channels@2022-09-15' = {
  parent: botService
  name: 'MsTeamsChannel'
  location: 'global'
  properties: {
    channelName: 'MsTeamsChannel'
    properties: {
      isEnabled: true
    }
  }
}

// Grant Bot Service Contributor permissions to Managed Identity
resource botServiceContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(botService.id, managedIdentity.id, 'BotContributor')
  scope: botService
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9fc6112f-f48e-4e27-8b09-72a5c94e4ae9') // Azure Bot Service Contributor Role
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output botServiceName string = botService.name
output managedIdentityName string = managedIdentity.name
output managedIdentityClientId string = managedIdentity.properties.clientId
output managedIdentityPrincipalId string = managedIdentity.properties.principalId
output containerRegistryName string = useContainer ? containerRegistry.name : ''
output containerRegistryLoginServer string = containerRegistryLoginServer
output containerBuildStatus string = (useContainer && autoBuildContainer) ? 'Build script executed' : 'Manual deployment required'
output resourceGroupName string = resourceGroup().name
