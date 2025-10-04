using System;
using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Command to change terminal color
/// </summary>
public class ColorCommand(MainWindow window, ITerminalService terminal) : ICommand
{
    public string[] Aliases => [];
    
    public string Name => "color";
    
    public string Description => "Change terminal color (green, amber, white, cyan)";

    public void Execute(string[] args)
    {
        if (args.Length > 0)
        {
            window.ChangeColor(args[0]);
        }
        else
        {
            terminal.WriteLine("Usage: color <green|amber|white|cyan>");
        }
    }
}