// <copyright file="ConfigOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Quote.Agent.Models;

/// <summary>
/// Configuration options for the bot.
/// Bot authentication is configured via Azure Web App environment variables.
/// </summary>
public class ConfigOptions
{
    // These properties are available for bot framework configuration
    public string MicrosoftAppId { get; set; } = string.Empty;

    public string MicrosoftAppPassword { get; set; } = string.Empty;

    public string MicrosoftAppTenantId { get; set; } = string.Empty;

    public string MicrosoftAppManagedIdentityResourceId { get; set; } = string.Empty;

    // OAuth configuration for Kusto authentication
    public string OAUTH_CONNECTION_NAME { get; set; } = string.Empty;

    public string AAD_APP_CLIENT_ID { get; set; } = string.Empty;

    public string AAD_APP_CLIENT_SECRET { get; set; } = string.Empty;

    public string AAD_APP_TENANT_ID { get; set; } = string.Empty;

    public string AAD_APP_OAUTH_AUTHORITY_HOST { get; set; } = string.Empty;
}