using System;
using System.Collections.Generic;
using System.Linq;

namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Registry and dispatcher for terminal commands
/// </summary>
public class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new();

    /// <summary>
    ///     Get all registered commands
    /// </summary>
    public IEnumerable<ICommand> GetAllCommands()
    {
        return _commands.Values.Distinct();
    }

    /// <summary>
    ///     Register a command
    /// </summary>
    public void Register(ICommand command)
    {
        _commands[command.Name.ToLower()] = command;

        foreach (var alias in command.Aliases) _commands[alias.ToLower()] = command;
    }

    /// <summary>
    ///     Execute a command by name with arguments
    /// </summary>
    public bool TryExecute(string commandLine)
    {
        var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return false;
        }

        var commandName = parts[0].ToLower();

        if (!_commands.TryGetValue(commandName, out var command))
        {
            return false;
        }
        
        var args = parts.Length > 1 ? parts[1..] : [];
        command.Execute(args);
        return true;
    }
}