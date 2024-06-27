using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.Interactions;

public sealed class InteractionCommandMapper
{
    private readonly FrozenDictionary<string, Command> fastCommandLookup;

    public InteractionCommandMapper(IReadOnlyList<Command> localCommands, IReadOnlyList<DiscordApplicationCommand> remoteCommands)
    {
        Dictionary<string, Command> commands = [];
        List<Command> unprocessedCommands = new(localCommands);
        foreach (DiscordApplicationCommand remoteCommand in remoteCommands)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(remoteCommand.Id);
            if (remoteCommand.Options != null)
            {
                BuildCommands(unprocessedCommands, remoteCommand.Options, stringBuilder);
            }

            if (commands.TryGetValue(stringBuilder.ToString(), out Command? command))
            {
                commands[stringBuilder.ToString()] = command;
                unprocessedCommands.Remove(command);
            }
        }
    }

    /// <summary>
    /// Attempts to resolve the command or nested subcommand from the interaction. Returns false if the command is not found.
    /// </summary>
    /// <param name="interaction">The interaction containing the command invocation data.</param>
    /// <param name="command">The resolved command, if found. Will be <see langword="default"/> when the method returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the command was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetCommand(DiscordInteraction interaction, [NotNullWhen(true)] out Command? command)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(interaction.Data.Id);
        ConstructSubCommandNames(interaction.Data.Options, stringBuilder);
        return this.fastCommandLookup.TryGetValue(stringBuilder.ToString(), out command);
    }

    private static void ConstructSubCommandNames(IReadOnlyList<DiscordInteractionDataOption> options, StringBuilder stringBuilder)
    {
        if (options.Count != 1)
        {
            return;
        }

        DiscordInteractionDataOption option = options[0];
        if (option.Type is not DiscordApplicationCommandOptionType.SubCommand and not DiscordApplicationCommandOptionType.SubCommandGroup)
        {
            return;
        }

        stringBuilder.Append('-');
        stringBuilder.Append(option.Name);
        ConstructSubCommandNames(option.Options, stringBuilder);
    }

    private static Dictionary<string, Command> BuildCommands(IReadOnlyList<Command> localCommands, IReadOnlyList<DiscordApplicationCommandOption> remoteCommands, StringBuilder stringBuilder)
    {

    }
}
