﻿using Elixus.Discord.Gateway.Contracts;
using Elixus.Discord.Gateway.Contracts.Events;

namespace Elixus.Discord.Gateway.Events.Handlers;

/// <summary>
/// Handles the <see cref="HeartbeatEvent" />.
/// This will ask the <see cref="IHeartbeatService" /> to immediately send a heartbeat without waiting.
/// </summary>
internal sealed class HeartbeatEventHandler : IEventHandler<HeartbeatEvent>
{
	/// <inheritdoc cref="IEventHandler{TEvent}.HandleEvent" />
	public ValueTask HandleEvent(HeartbeatEvent @event, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}