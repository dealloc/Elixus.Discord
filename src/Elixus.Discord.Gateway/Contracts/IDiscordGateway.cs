using Elixus.Discord.Gateway.Events.Base;

namespace Elixus.Discord.Gateway.Contracts;

/// <summary>
/// Provides interactions with the Discord WS Gateway.
/// </summary>
/// <remarks>
/// You normally shouldn't need to access this directly.
/// </remarks>
public interface IDiscordGateway
{
	/// <summary>
	/// Starts the <see cref="IDiscordGateway" /> and run it until the <paramref name="cancellationToken" /> is set.
	/// </summary>
	/// <param name="endpoint">The websocket gateway to connect to.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken" /> to stop the server.</param>
	Task RunAsync(Uri endpoint, CancellationToken cancellationToken = default);

	Task SendAsync(ArraySegment<byte> payload, CancellationToken cancellationToken);
}