using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordRoleConverter : ISlashArgumentConverter<DiscordRole>, ITextArgumentConverter<DiscordRole>
{
    [GeneratedRegex(@"^<@&(\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex getRoleRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Role;
    public string ReadableName => "Discord Role";
    public bool RequiresText => true;

    public Task<Optional<DiscordRole>> ConvertAsync(ConverterContext context)
    {
        if (context is InteractionConverterContext interactionContext
            && interactionContext.Interaction.Data.Resolved is not null
            && ulong.TryParse(interactionContext.Argument?.RawValue, CultureInfo.InvariantCulture, out ulong roleId)
            && interactionContext.Interaction.Data.Resolved.Roles.TryGetValue(roleId, out DiscordRole? role))
        {
            return Task.FromResult(Optional.FromValue(role));
        }

        // We can't get a role if there's not a guild to look in.
        if (context.Guild is null)
        {
            return Task.FromResult(Optional.FromNoValue<DiscordRole>());
        }

        string? value = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(Optional.FromNoValue<DiscordRole>());
        }

        // Try parsing the value as a role id.
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out roleId))
        {
            // value can be a raw channel id or a channel mention. The regex will match both.
            Match match = getRoleRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out roleId))
            {
                // Attempt to find a role by name, case sensitive.
                DiscordRole? namedRole = context.Guild.Roles.Values.FirstOrDefault(role => role.Name.Equals(value, StringComparison.Ordinal));
                return Task.FromResult(namedRole is not null ? Optional.FromValue(namedRole) : Optional.FromNoValue<DiscordRole>());
            }
        }

        // Return the role if it exists in the guild.
        return context.Guild.GetRole(roleId) is DiscordRole guildRole
            ? Task.FromResult(Optional.FromValue(guildRole))
            : Task.FromResult(Optional.FromNoValue<DiscordRole>());
    }
}
