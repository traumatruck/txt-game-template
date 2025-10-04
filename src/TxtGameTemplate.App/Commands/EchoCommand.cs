using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Command to echo text back to the terminal
/// </summary>
public class EchoCommand(ITerminalService terminal) : ICommand
{
    public string[] Aliases => [];
    
    public string Name => "echo";
    
    public string Description => "Echo text back to the terminal";

    public void Execute(string[] args)
    {
        if (args.Length > 0)
        {
            terminal.WriteLine(string.Join(" ", args));
        }
    }
}