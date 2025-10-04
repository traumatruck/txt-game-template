using System.Collections.Generic;
using System.Text;
using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Command to demonstrate a multi-panel UI interface with arrow key navigation
/// </summary>
public class PanelsCommand(MainWindow window) : ICommand
{
    public string Name => "panels";
    
    public string[] Aliases => [];
    
    public string Description => "Show multi-panel UI demo with arrow key navigation";

    public void Execute(string[] args)
    {
        window.ShowPanelDemo();
    }
}

/// <summary>
///     Panel data for the multi-panel demo
/// </summary>
public class Panel
{
    public string Title { get; set; } = string.Empty;
    public List<string> Content { get; set; } = [];
    public int Row { get; set; }
    public int Col { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
///     Panel layout manager for the demo
/// </summary>
public class PanelLayout
{
    private readonly List<Panel> _panels = [];
    private int _activePanelIndex = 0;

    public PanelLayout()
    {
        InitializePanels();
    }

    private void InitializePanels()
    {
        // Top panel - Status bar
        _panels.Add(new Panel
        {
            Title = "System Status",
            Content = 
            [
                "CPU: 45%  Memory: 2.3GB  Disk: 128GB Free",
                "Network: Connected  Uptime: 5d 12h 34m",
                "Status: All systems operational"
            ],
            Row = 0,
            Col = 0,
            Width = 60,
            Height = 5,
            IsActive = true
        });

        // Left panel - Navigation
        _panels.Add(new Panel
        {
            Title = "Navigation",
            Content = 
            [
                "► Dashboard",
                "  Inventory",
                "  Statistics",
                "  Settings",
                "  Help",
                "",
                "Use arrows to",
                "navigate panels"
            ],
            Row = 6,
            Col = 0,
            Width = 20,
            Height = 12,
            IsActive = false
        });

        // Center panel - Main content
        _panels.Add(new Panel
        {
            Title = "Main Content - Dashboard",
            Content = 
            [
                "Welcome to the Multi-Panel Demo!",
                "",
                "This demonstrates a complex UI built",
                "with ASCII art that can be navigated",
                "using arrow keys.",
                "",
                "Features:",
                "• Multiple independent panels",
                "• Arrow key navigation between panels",
                "• Active panel highlighting",
                "• Responsive ASCII borders",
                "• Organized content display"
            ],
            Row = 6,
            Col = 22,
            Width = 38,
            Height = 12,
            IsActive = false
        });

        // Bottom panel - Log/Messages
        _panels.Add(new Panel
        {
            Title = "Activity Log",
            Content = 
            [
                "[10:45:23] System initialized",
                "[10:45:24] Panels loaded successfully",
                "[10:45:25] Ready for input",
                "[10:45:26] Use arrows to switch panels",
                "[10:45:27] Press ESC to exit demo"
            ],
            Row = 19,
            Col = 0,
            Width = 60,
            Height = 8,
            IsActive = false
        });
    }

    public void MoveNext()
    {
        _panels[_activePanelIndex].IsActive = false;
        _activePanelIndex = (_activePanelIndex + 1) % _panels.Count;
        _panels[_activePanelIndex].IsActive = true;
    }

    public void MovePrevious()
    {
        _panels[_activePanelIndex].IsActive = false;
        _activePanelIndex--;
        if (_activePanelIndex < 0)
            _activePanelIndex = _panels.Count - 1;
        _panels[_activePanelIndex].IsActive = true;
    }

    public string Render()
    {
        var sb = new StringBuilder();
        var grid = new Dictionary<(int row, int col), char>();

        // Render each panel to the grid
        foreach (var panel in _panels)
        {
            RenderPanel(panel, grid);
        }

        // Convert grid to string
        var maxRow = 0;
        var maxCol = 0;
        foreach (var key in grid.Keys)
        {
            if (key.row > maxRow) maxRow = key.row;
            if (key.col > maxCol) maxCol = key.col;
        }

        for (var row = 0; row <= maxRow; row++)
        {
            for (var col = 0; col <= maxCol; col++)
            {
                sb.Append(grid.TryGetValue((row, col), out var ch) ? ch : ' ');
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Use ← → arrows to switch panels | ESC to exit");

        return sb.ToString();
    }

    private void RenderPanel(Panel panel, Dictionary<(int row, int col), char> grid)
    {
        var activeChar = panel.IsActive ? '═' : '─';
        var activeCorner = panel.IsActive ? '╔' : '┌';
        var activeCornerTR = panel.IsActive ? '╗' : '┐';
        var activeCornerBL = panel.IsActive ? '╚' : '└';
        var activeCornerBR = panel.IsActive ? '╝' : '┘';
        var activeVert = panel.IsActive ? '║' : '│';

        // Top border
        grid[(panel.Row, panel.Col)] = activeCorner;
        for (var i = 1; i < panel.Width - 1; i++)
        {
            grid[(panel.Row, panel.Col + i)] = activeChar;
        }
        grid[(panel.Row, panel.Col + panel.Width - 1)] = activeCornerTR;

        // Title
        var title = $" {panel.Title} ";
        if (panel.IsActive) title = $"►{panel.Title}◄";
        var titleStart = (panel.Width - title.Length) / 2;
        for (var i = 0; i < title.Length && titleStart + i < panel.Width - 1; i++)
        {
            grid[(panel.Row, panel.Col + titleStart + i)] = title[i];
        }

        // Content area
        for (var row = 1; row < panel.Height - 1; row++)
        {
            grid[(panel.Row + row, panel.Col)] = activeVert;
            
            // Content line
            if (row - 1 < panel.Content.Count)
            {
                var content = panel.Content[row - 1];
                for (var i = 0; i < content.Length && i < panel.Width - 3; i++)
                {
                    grid[(panel.Row + row, panel.Col + 1 + i)] = content[i];
                }
            }
            
            grid[(panel.Row + row, panel.Col + panel.Width - 1)] = activeVert;
        }

        // Bottom border
        grid[(panel.Row + panel.Height - 1, panel.Col)] = activeCornerBL;
        for (var i = 1; i < panel.Width - 1; i++)
        {
            grid[(panel.Row + panel.Height - 1, panel.Col + i)] = activeChar;
        }
        grid[(panel.Row + panel.Height - 1, panel.Col + panel.Width - 1)] = activeCornerBR;
    }
}
