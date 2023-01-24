using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Elixus.Discord.Gateway.Constants;
using Elixus.Discord.Gateway.Contracts.Events;
using Elixus.Discord.Gateway.Events.Base;
using Elixus.Discord.Gateway.Parsing;

namespace Elixus.Discord.Gateway.Events.Serializers;

/// <summary>
/// Serializes the <see cref="IdentifyEvent" /> for receiving and sending from the gateway.
/// </summary>
public sealed class IdentifyEventSerializer : IEventSerializer<IdentifyEvent>
{
	private readonly JsonTypeInfo<IdentifyEvent> _identifyType = GatewayEventSerializerContext.Default.IdentifyEvent;
	private readonly JsonTypeInfo<GatewayEvent<IdentifyEvent>> _gatewayType = GatewayEventSerializerContext.Default.GatewayEventIdentifyEvent;

	/// <inheritdoc cref="IEventSerializer{TEvent}.Serialize" />
	public ArraySegment<byte> Serialize(IdentifyEvent @event)
		=> JsonSerializer.SerializeToUtf8Bytes(new GatewayEvent<IdentifyEvent>
		{
			Opcode = GatewayOpcodes.Identify,
			Event = @event
		}, _gatewayType);

	/// <inheritdoc cref="IEventSerializer{TEvent}.Deserialize" />
	public IdentifyEvent Deserialize(ReadOnlySpan<byte> payload)
		=> JsonSerializer.Deserialize(payload, _identifyType)!;
}