﻿using Elixus.Discord.Gateway.Contracts;
using Elixus.Discord.Gateway.Events;

namespace Elixus.Discord.Gateway.EventHandlers;

/// <summary>
/// Handles the <see cref="ReconnectEvent" />.
/// This handler will attempt to stop the gateway and force a resume.
/// </summary>
internal sealed class ReconnectEventHandler : IEventHandler<ReconnectEvent>
{
	/// <inheritdoc cref="IEventHandler{TEvent}.HandleEvent" />
	public ValueTask HandleEvent(ReconnectEvent @event, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}