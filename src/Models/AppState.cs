// <copyright file="AppState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.Teams.AI.State;

namespace Quote.Agent.Models;

// Extend the turn state by configuring custom strongly typed state classes.
public class AppState : TurnState
{
    public AppState()
        : base()
    {
        this.ScopeDefaults[CONVERSATION_SCOPE] = new ConversationState();
    }

    /// <summary>
    /// Gets or sets stores all the conversation-related state.
    /// </summary>
    public new ConversationState Conversation
    {
        get
        {
            TurnStateEntry? scope = this.GetScope(CONVERSATION_SCOPE);
            if (scope == null)
            {
                throw new ArgumentException("TurnState hasn't been loaded. Call LoadStateAsync() first.");
            }

            return (ConversationState)scope.Value!;
        }

        set
        {
            TurnStateEntry? scope = this.GetScope(CONVERSATION_SCOPE);
            if (scope == null)
            {
                throw new ArgumentException("TurnState hasn't been loaded. Call LoadStateAsync() first.");
            }

            scope.Replace(value!);
        }
    }
}

// This class adds custom properties to the turn state which will be accessible in the activity handler methods.
public class ConversationState : Record
{
    public int MessageCount
    {
        get => this.Get<int>("MessageCount");
        set => this.Set("MessageCount", value);
    }
}