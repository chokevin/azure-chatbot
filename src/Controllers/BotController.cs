// <copyright file="BotController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Quote.Agent.Controllers;

[Route("api/messages")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly IBotFrameworkHttpAdapter adapter;
    private readonly IBot bot;

    public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
    {
        this.adapter = adapter;
        this.bot = bot;
    }

    [HttpPost]
    public async Task PostAsync(CancellationToken cancellationToken = default)
    {
        await this.adapter.ProcessAsync(
            this.Request,
            this.Response,
            this.bot,
            cancellationToken);
    }
}