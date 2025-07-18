# Script to create Azure AD App Registration for Bot Service
# This script creates an App Registration with the required permissions for a Bot Service

param(
    [Parameter(Mandatory=$false)]
    [string]$DisplayName = "Azure Bot Sample for Teams",
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "Echo bot with Teams integration built with .NET"
)

# Check if Azure CLI is installed and user is logged in
try {
    $account = az account show --query "user.name" -o tsv 2>$null
    if (-not $account) {
        Write-Error "Please log in to Azure CLI first: az login"
        exit 1
    }
    Write-Host "✓ Logged in to Azure as: $account" -ForegroundColor Green
} catch {
    Write-Error "Azure CLI is not installed or not accessible. Please install Azure CLI first."
    exit 1
}

# Create App Registration
Write-Host "Creating Azure AD App Registration..." -ForegroundColor Yellow

try {
    # Create the app registration
    $appRegistration = az ad app create `
        --display-name $DisplayName `
        --sign-in-audience "AzureADMultipleOrgs" `
        --query "{appId:appId,objectId:id}" -o json | ConvertFrom-Json
    
    $appId = $appRegistration.appId
    $objectId = $appRegistration.objectId
    
    Write-Host "✓ Created App Registration with App ID: $appId" -ForegroundColor Green
    
    # Create a client secret
    $expirationDate = (Get-Date).AddYears(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $secret = az ad app credential reset `
        --id $appId `
        --years 2 `
        --query "password" -o tsv
    
    Write-Host "✓ Created client secret (expires in 2 years)" -ForegroundColor Green
    
    # Set required API permissions for Bot Framework
    Write-Host "Setting API permissions..." -ForegroundColor Yellow
    
    # Microsoft Graph permissions for bot
    az ad app permission add --id $appId --api 00000003-0000-0000-c000-000000000000 --api-permissions e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope
    
    Write-Host "✓ Added Microsoft Graph permissions" -ForegroundColor Green
    
    # Output environment variables
    Write-Host "`n" -ForegroundColor Yellow
    Write-Host "App Registration created successfully!" -ForegroundColor Green
    Write-Host "Please set the following environment variables:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "For Windows (PowerShell):" -ForegroundColor Cyan
    Write-Host "`$env:BOT_APP_ID = '$appId'" -ForegroundColor White
    Write-Host "`$env:BOT_APP_PASSWORD = '$secret'" -ForegroundColor White
    Write-Host "`$env:BOT_APP_TYPE = 'MultiTenant'" -ForegroundColor White
    Write-Host "`$env:BOT_APP_TENANT_ID = ''" -ForegroundColor White
    Write-Host ""
    Write-Host "For Linux/macOS (Bash):" -ForegroundColor Cyan
    Write-Host "export BOT_APP_ID='$appId'" -ForegroundColor White
    Write-Host "export BOT_APP_PASSWORD='$secret'" -ForegroundColor White
    Write-Host "export BOT_APP_TYPE='MultiTenant'" -ForegroundColor White
    Write-Host "export BOT_APP_TENANT_ID=''" -ForegroundColor White
    Write-Host ""
    Write-Host "Or add them to your .env file:" -ForegroundColor Cyan
    Write-Host "BOT_APP_ID=$appId" -ForegroundColor White
    Write-Host "BOT_APP_PASSWORD=$secret" -ForegroundColor White
    Write-Host "BOT_APP_TYPE=MultiTenant" -ForegroundColor White
    Write-Host "BOT_APP_TENANT_ID=" -ForegroundColor White
    Write-Host ""
    Write-Host "⚠️  IMPORTANT: Save the client secret securely. It cannot be retrieved again!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Set the environment variables above" -ForegroundColor White
    Write-Host "2. Run 'azd up' to deploy your bot to Azure" -ForegroundColor White
    Write-Host "3. Follow the Teams deployment guide in TEAMS-DEPLOYMENT.md" -ForegroundColor White
    
} catch {
    Write-Error "Failed to create App Registration: $_"
    exit 1
}
