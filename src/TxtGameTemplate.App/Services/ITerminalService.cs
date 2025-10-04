namespace TxtGameTemplate.App.Services;

/// <summary>
///     Interface for terminal output operations
/// </summary>
public interface ITerminalService
{
    /// <summary>
    ///     Clear the terminal screen
    /// </summary>
    void Clear();

    /// <summary>
    ///     Write a line of text to the terminal
    /// </summary>
    void WriteLine(string text);
}