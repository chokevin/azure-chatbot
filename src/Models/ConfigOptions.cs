// <copyright file="ConfigOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Quote.Agent.Models;

/// <summary>
/// Configuration options for the bot.
/// Note: Bot authentication is configured via Azure Web App environment variables,
/// not through this class. This is kept for potential future extensions.
/// </summary>
public class ConfigOptions
{
    // These properties are available for future use but authentication
    // is handled automatically by Azure Bot Framework with Managed Identity
    public string MicrosoftAppId { get; set; } = string.Empty;

    public string MicrosoftAppPassword { get; set; } = string.Empty;

    public string MicrosoftAppTenantId { get; set; } = string.Empty;

    public string MicrosoftAppManagedIdentityResourceId { get; set; } = string.Empty;

    public string OAUTH_CONNECTION_NAME { get; set; } = "entra";
}