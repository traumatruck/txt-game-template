namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Command to show interactive menu
/// </summary>
public class MenuCommand(MainWindow window) : ICommand
{
    public string[] Aliases => [];

    public string Name => "menu";
    
    public string Description => "Show interactive menu with arrow key navigation";

    public void Execute(string[] args)
    {
        window.ShowDemoMenu();
    }
}