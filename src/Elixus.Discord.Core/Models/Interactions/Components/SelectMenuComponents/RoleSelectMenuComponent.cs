using Elixus.Discord.Core.Constants.Interactions;

namespace Elixus.Discord.Core.Models.Interactions.Components.SelectMenuComponents;

/// <see href="https://discord.com/developers/docs/interactions/message-components#select-menu-object-select-menu-structure" />
public class RoleSelectMenuComponent : SelectMenuComponent
{
	/// <inheritdoc cref="Component.Type" />
	public override ComponentTypes Type { get; set; } = ComponentTypes.RoleSelect;
}