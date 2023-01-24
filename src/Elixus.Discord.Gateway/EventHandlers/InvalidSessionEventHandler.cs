﻿using Elixus.Discord.Gateway.Contracts;
using Elixus.Discord.Gateway.Events;

namespace Elixus.Discord.Gateway.EventHandlers;

/// <summary>
/// Handles the <see cref="InvalidSessionEvent" />.
/// This handler will stop the gateway and resume if the event allows it.
/// </summary>
internal sealed class InvalidSessionEventHandler : IEventHandler<InvalidSessionEvent>
{
	/// <inheritdoc cref="IEventHandler{TEvent}.HandleEvent" />
	public ValueTask HandleEvent(InvalidSessionEvent @event, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}