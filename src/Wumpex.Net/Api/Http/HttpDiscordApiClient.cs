﻿using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wumpex.Net.Api.Contracts;
using Wumpex.Net.Api.Exceptions;
using Wumpex.Net.Api.Models.Channels;
using Wumpex.Net.Api.Models.Gateway;
using Wumpex.Net.Api.Models.Interactions.ApplicationCommands;
using Wumpex.Net.Api.Models.Interactions.InteractionResponses;
using Wumpex.Net.Api.Models.Webhooks;
using Wumpex.Net.Api.Serialization;
using Wumpex.Net.Core.Configurations;
using Wumpex.Net.Core.Models.Channels;
using Wumpex.Net.Core.Models.Interactions;
using Wumpex.Net.Core.Models.Interactions.ApplicationCommands;
using Wumpex.Net.Core.Models.Users;
using Wumpex.Net.Core.Serialization;

namespace Wumpex.Net.Api.Http;

/// <summary>
/// Implements the <see cref="IDiscordApi" /> contract using the <see cref="HttpClient" />.
/// </summary>
internal sealed class HttpDiscordApiClient : IDiscordApi
{
	private readonly HttpClient _http;
	private readonly ILogger<HttpDiscordApiClient> _logger;
	private readonly IOptionsMonitor<DiscordConfiguration> _monitor;

	private const string _endpoint = "https://discord.com/api/v10";

	public HttpDiscordApiClient(ILogger<HttpDiscordApiClient> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<DiscordConfiguration> monitor)
	{
		_logger = logger;
		_monitor = monitor;
		_http = httpClientFactory.CreateClient("wumpex.net.api");
	}

	/// <inheritdoc cref="IDiscordApi.GetGatewayBotAsync" />
	public async Task<GatewayBotResponse> GetGatewayBotAsync(CancellationToken cancellationToken = default)
	{
		var response = await _http.GetFromJsonAsync<GatewayBotResponse>($"{_endpoint}/gateway/bot", cancellationToken);

		return response ?? throw new UnexpectedResponseException("/gateway/bot");
	}

	/// <inheritdoc cref="IDiscordApi.GetCurrentUser" />
	public async Task<User> GetCurrentUser(CancellationToken cancellationToken = default)
	{
		var response = await _http.GetFromJsonAsync<User>($"{_endpoint}/users/@me", cancellationToken);

		return response ?? throw new UnexpectedResponseException("/users/@me");
	}

	/// <inheritdoc cref="IDiscordApi.CreateGlobalApplicationCommand" />
	public async Task<ApplicationCommand> CreateGlobalApplicationCommand(CreateApplicationCommandRequest request, CancellationToken cancellationToken = default)
	{
		var response = await _http.PostAsJsonAsync($"{_endpoint}/applications/{_monitor.CurrentValue.ApplicationId}/commands", request, cancellationToken);

		try
		{
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync(ApiSerializerContext.Default.ApplicationCommand, cancellationToken)  ?? throw new UnexpectedResponseException($"{_endpoint}/applications/{_monitor.CurrentValue.ApplicationId}/commands");
		}
		catch (HttpRequestException exception) when (response.Content.Headers.ContentLength > 0)
		{
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogError(exception, "Failed to create global application: {Message}", body);
			throw;
		}
	}

	/// <inheritdoc cref="IDiscordApi.GetGlobalApplicationCommands" />
	public async IAsyncEnumerable<ApplicationCommand> GetGlobalApplicationCommands(bool withLocalizations = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = await _http.GetStreamAsync($"{_endpoint}/applications/{_monitor.CurrentValue.ApplicationId}/commands", cancellationToken);

		await foreach (var command in JsonSerializer.DeserializeAsyncEnumerable(stream, ApiSerializerContext.Default.ApplicationCommand, cancellationToken))
		{
			yield return command!;
		}
	}

	/// <inheritdoc cref="IDiscordApi.DeleteGlobalApplicationCommand" />
	public async Task DeleteGlobalApplicationCommand(string id, CancellationToken cancellationToken = default)
	{
		var response = await _http.DeleteAsync($"{_endpoint}/applications/{_monitor.CurrentValue.ApplicationId}/commands/{id}", cancellationToken);

		response.EnsureSuccessStatusCode();
	}

	/// <inheritdoc cref="IDiscordApi.CreateMessage" />
	public async Task<Message> CreateMessage(string channel, CreateMessageRequest request, CancellationToken cancellationToken = default)
	{
		var response = await _http.PostAsJsonAsync($"{_endpoint}/channels/{channel}/messages", request, cancellationToken);

		try
		{
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync(EventSerializerContext.Default.Message, cancellationToken) ?? throw new UnexpectedResponseException($"{_endpoint}/channels/{channel}/messages");
		}
		catch (HttpRequestException exception) when (response.Content.Headers.ContentLength > 0)
		{
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogError(exception, "Failed to create global application: {Message}", body);
			throw;
		}
	}

	/// <inheritdoc cref="IDiscordApi.RespondToInteraction" />
	public async Task RespondToInteraction(Interaction interaction, InteractionResponse reply, CancellationToken cancellationToken = default)
	{
		var response = await _http.PostAsJsonAsync($"{_endpoint}/interactions/{interaction.Id}/{interaction.Token}/callback", reply, cancellationToken: cancellationToken);

		try
		{
			response.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException exception) when (response.Content.Headers.ContentLength > 0)
		{
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogError(exception, "Failed to respond to interaction {Id}: {Message}", interaction.Id, body);
			throw;
		}
	}

	/// <inheritdoc cref="IDiscordApi.FollowUpInteraction" />
	public async Task<Message> FollowUpInteraction(Interaction interaction, ExecuteWebhookRequest request, CancellationToken cancellationToken = default)
	{
		var requestUri = $"{_endpoint}/webhooks/{_monitor.CurrentValue.ApplicationId}/{interaction.Token}";
		if (string.IsNullOrWhiteSpace(request.ThreadId) is false)
			requestUri = $"{requestUri}?thread_id={request.ThreadId}";
		if (request.Wait is not null)
			requestUri = $"{requestUri}{(requestUri.Contains('?') ? '&' : '?')}wait={request.Wait}";

		var response = await _http.PostAsJsonAsync(requestUri, request, cancellationToken: cancellationToken);

		try
		{
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<Message>(cancellationToken: cancellationToken) ?? throw new UnexpectedResponseException(requestUri);
		}
		catch (HttpRequestException exception) when (response.Content.Headers.ContentLength > 0)
		{
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogError(exception, "Failed to follow up on interaction {Id}: {Message}", interaction.Id, body);
			throw;
		}
	}

	public async Task EditFollowUpInteraction(Interaction interaction, Message message, EditWebhookMessageRequest request, CancellationToken cancellationToken = default)
	{
		var requestUri = $"{_endpoint}/webhooks/{_monitor.CurrentValue.ApplicationId}/{interaction.Token}";
		if (string.IsNullOrWhiteSpace(request.ThreadId) is false)
			requestUri = $"{requestUri}?thread_id={request.ThreadId}";

		var response = await _http.PostAsJsonAsync(requestUri, request, cancellationToken: cancellationToken);

		try
		{
			response.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException exception) when (response.Content.Headers.ContentLength > 0)
		{
			var body = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogError(exception, "Failed to edit follow up on interaction {Id}: {Message}", interaction.Id, body);
			throw;
		}
	}
}
