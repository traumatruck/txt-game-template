namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Base interface for terminal commands
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     Optional aliases for the command (e.g., "cls" for "clear")
    /// </summary>
    string[] Aliases { get; }

    /// <summary>
    ///     The command name (e.g., "help", "rps", "menu")
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Brief description of what the command does
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     Execute the command with the provided arguments
    /// </summary>
    /// <param name="args">Command arguments (not including the command name itself)</param>
    void Execute(string[] args);
}