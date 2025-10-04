using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App.Commands;

/// <summary>
/// Command to display help information
/// </summary>
public class HelpCommand(ITerminalService terminal, CommandRegistry registry) : ICommand
{
    public string Name => "help";
    
    public string[] Aliases => [];
    
    public string Description => "Show this help message with all available commands";

    public void Execute(string[] args)
    {
        terminal.WriteLine("Available commands:");
        
        foreach (var command in registry.GetAllCommands())
        {
            var aliasText = command.Aliases.Length > 0 
                ? $" / {string.Join(" / ", command.Aliases)}" 
                : "";
            
            terminal.WriteLine($"  {command.Name}{aliasText} - {command.Description}");
        }
    }
}
