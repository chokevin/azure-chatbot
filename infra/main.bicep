// Azure Bot Service with Teams Integration - Main Infrastructure Template
// This template creates all necessary Azure resources for deploying a bot to Teams

targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment that will be used to name all resources')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

// Bot configuration parameters
@secure()
@description('Microsoft App ID for the bot (will be generated if not provided)')
param microsoftAppId string = ''

@secure()
@description('Microsoft App Password for the bot (will be generated if not provided)')
param microsoftAppPassword string = ''

@description('Microsoft App Tenant ID for the bot (optional - leave empty for MultiTenant)')
param microsoftAppTenantId string = ''

@description('Microsoft App Type for the bot')
@allowed(['MultiTenant', 'SingleTenant', 'UserAssignedMSI'])
param microsoftAppType string = 'MultiTenant'

@description('Bot display name')
param botDisplayName string = 'Azure Bot Sample'

@description('Bot description')
param botDescription string = 'Echo bot for Microsoft Teams integration'

@description('App Service Plan SKU')
@allowed(['F1', 'D1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2'])
param appServicePlanSku string = 'F1'

// Generate resource token for unique naming
var resourceToken = toLower(uniqueString(subscription().id, resourceGroup().id, environmentName))
var resourcePrefix = 'bot'
var tags = {
  'azd-env-name': environmentName
  purpose: 'bot-teams-integration'
  environment: environmentName
}

// Create resource names using the pattern {resourcePrefix}-{resourceToken}
var appServicePlanName = '${resourcePrefix}-plan-${resourceToken}'
var appServiceName = '${resourcePrefix}-app-${resourceToken}'
var botServiceName = '${resourcePrefix}-service-${resourceToken}'
var applicationInsightsName = '${resourcePrefix}-ai-${resourceToken}'
var logAnalyticsWorkspaceName = '${resourcePrefix}-law-${resourceToken}'
var keyVaultName = '${resourcePrefix}-kv-${resourceToken}'
var managedIdentityName = '${resourcePrefix}-mi-${resourceToken}'

// Resource group is the deployment target scope

// Create user-assigned managed identity for secure access
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: tags
}

// Create Log Analytics Workspace for monitoring
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Create Application Insights for telemetry and monitoring
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    DisableIpMasking: false
    DisableLocalAuth: false
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Create Key Vault for storing secrets
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enableRbacAuthorization: true
    publicNetworkAccess: 'Enabled'
    accessPolicies: []
  }
}

// Grant managed identity access to Key Vault secrets
resource keyVaultSecretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, managedIdentity.id, '4633458b-17de-408a-b874-0445c86b69e6')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Store bot secrets in Key Vault
resource botAppPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(microsoftAppPassword)) {
  parent: keyVault
  name: 'bot-app-password'
  properties: {
    value: microsoftAppPassword
    contentType: 'text/plain'
  }
}

resource botAppIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(microsoftAppId)) {
  parent: keyVault
  name: 'bot-app-id'
  properties: {
    value: microsoftAppId
    contentType: 'text/plain'
  }
}

// Create App Service Plan for hosting the bot
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: appServicePlanSku
  }
  properties: {
    reserved: false // Windows
  }
}

// Create App Service for the bot application
resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location
  tags: union(tags, {
    'azd-service-name': 'bot-service'
  })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      alwaysOn: appServicePlanSku != 'F1' && appServicePlanSku != 'D1' // AlwaysOn not supported on Free tier
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'MicrosoftAppType'
          value: microsoftAppType
        }
        {
          name: 'MicrosoftAppId'
          value: microsoftAppType == 'UserAssignedMSI' ? managedIdentity.properties.clientId : (!empty(microsoftAppId) ? microsoftAppId : '')
        }
        {
          name: 'MicrosoftAppPassword'
          value: !empty(microsoftAppPassword) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=bot-app-password)' : ''
        }
        {
          name: 'MicrosoftAppTenantId'
          value: microsoftAppTenantId
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      http20Enabled: true
      healthCheckPath: '/health'
    }
  }
}

// Create diagnostic settings for App Service
resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: appService
  name: 'diagnostics'
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

// Create Azure Bot Service
resource botService 'Microsoft.BotService/botServices@2022-09-15' = {
  name: botServiceName
  location: 'global' // Bot Service is a global resource
  tags: tags
  sku: {
    name: 'F0' // Free tier for development
  }
  kind: 'azurebot'
  properties: {
    displayName: botDisplayName
    description: botDescription
    endpoint: 'https://${appService.properties.defaultHostName}/api/messages'
    msaAppType: microsoftAppType
    msaAppId: microsoftAppType == 'UserAssignedMSI' ? managedIdentity.properties.clientId : (!empty(microsoftAppId) ? microsoftAppId : '')
    msaAppTenantId: microsoftAppType == 'MultiTenant' ? '' : microsoftAppTenantId
    iconUrl: 'https://docs.botframework.com/static/devportal/client/images/bot-framework-default.png'
    developerAppInsightKey: applicationInsights.properties.InstrumentationKey
    developerAppInsightsApiKey: '' // Will be configured post-deployment
    developerAppInsightsApplicationId: applicationInsights.properties.AppId
    isStreamingSupported: true
    publicNetworkAccess: 'Enabled'
  }
}

// Enable Microsoft Teams channel for the bot
resource teamsChannel 'Microsoft.BotService/botServices/channels@2022-09-15' = {
  parent: botService
  name: 'MsTeamsChannel'
  location: 'global'
  properties: {
    channelName: 'MsTeamsChannel'
    properties: {
      enableCalling: false
      isEnabled: true
    }
  }
}

// Enable Web Chat channel for testing
resource webChatChannel 'Microsoft.BotService/botServices/channels@2022-09-15' = {
  parent: botService
  name: 'WebChatChannel'
  location: 'global'
  properties: {
    channelName: 'WebChatChannel'
    properties: {
      sites: [
        {
          siteName: 'Default Site'
          isEnabled: true
          isV1Enabled: true
          isV3Enabled: true
        }
      ]
    }
  }
}

// Outputs for azd and deployment
output BOT_APP_SERVICE_NAME string = appServiceName
output BOT_SERVICE_NAME string = botServiceName
output BOT_ENDPOINT string = 'https://${appService.properties.defaultHostName}/api/messages'
output APPLICATION_INSIGHTS_CONNECTION_STRING string = applicationInsights.properties.ConnectionString
output KEY_VAULT_NAME string = keyVaultName
output RESOURCE_GROUP_NAME string = resourceGroup().name
output RESOURCE_GROUP_ID string = resourceGroup().id
output BOT_SERVICE_RESOURCE_ID string = botService.id
output TEAMS_CHANNEL_CONFIGURED bool = true
output APP_SERVICE_URL string = 'https://${appService.properties.defaultHostName}'
output LOG_ANALYTICS_WORKSPACE_ID string = logAnalyticsWorkspace.id
output MANAGED_IDENTITY_CLIENT_ID string = managedIdentity.properties.clientId
output MANAGED_IDENTITY_NAME string = managedIdentityName
