using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Command to clear the terminal screen
/// </summary>
public class ClearCommand(ITerminalService terminal) : ICommand
{
    public string[] Aliases => ["cls"];
    
    public string Name => "clear";
    
    public string Description => "Clear the terminal screen";

    public void Execute(string[] args)
    {
        terminal.Clear();
    }
}