using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace TxtGameTemplate.App;

public partial class MainWindow : Window
{
    private readonly List<string> _commandHistory = [];
    private int _historyIndex = -1;
    private readonly StringBuilder _terminalText = new();
    private readonly Random _random = new();
    
    // RPS Game state
    private int _rpsWins = 0;
    private int _rpsLosses = 0;
    private int _rpsTies = 0;
    
    // Menu system state
    private bool _inMenuMode = false;
    private List<MenuItem> _currentMenuItems = [];
    private int _selectedMenuIndex = 0;
    private int _menuContentStartLength = 0;

    public MainWindow()
    {
        InitializeComponent();
        InitializeTerminal();

        // Ensure focus is set after window is fully loaded
        Opened += OnWindowOpened;
    }

    private void ChangeColor(string color)
    {
        var colorValue = color.ToLower() switch
        {
            "green" => "#00FF00",
            "amber" => "#FFB000",
            "white" => "#FFFFFF",
            "cyan" => "#00FFFF",
            _ => null
        };

        if (colorValue != null)
        {
            var brush = new SolidColorBrush(Color.Parse(colorValue));
            
            // Change text colors
            TerminalOutput.Foreground = brush;
            CommandInput.Foreground = brush;
            
            // Change caret color
            CommandInput.CaretBrush = brush;
            
            // Change prompt color
            var promptText = this.FindControl<TextBlock>("Prompt");

            if (promptText != null)
            {
                promptText.Foreground = brush;
            }
            
            // Change separator border color
            var separatorBorder = this.FindControl<Border>("SeparatorBorder");

            if (separatorBorder != null)
            {
                separatorBorder.BorderBrush = brush;
            }
            
            WriteToTerminal($"Color changed to {color}");
        }
        else
        {
            WriteToTerminal($"Unknown color: {color}");
            WriteToTerminal("Available colors: green, amber, white, cyan");
        }
    }

    private void ClearTerminal()
    {
        _terminalText.Clear();
        TerminalOutput.Text = string.Empty;
    }

    private void InitializeTerminal()
    {
        // Display welcome message
        WriteToTerminal("╔═══════════════════════════════════════════════════════════╗");
        WriteToTerminal("║          TEXT GAME FRAMEWORK v1.0                         ║");
        WriteToTerminal("║          Retro Style Terminal                             ║");
        WriteToTerminal("╚═══════════════════════════════════════════════════════════╝");
        WriteToTerminal("");
        WriteToTerminal("Type 'help' for available commands.");
        WriteToTerminal("");

        // Set up event handlers
        CommandInput.KeyDown += OnCommandInputKeyDown;
        CommandInput.LostFocus += OnCommandInputLostFocus;
        
        // Handle keyboard input at window level for menu navigation
        this.KeyDown += OnWindowKeyDown;

        // Prevent focus from leaving the command input
        TerminalOutput.Focusable = false;
        TerminalOutput.IsReadOnly = true;
    }
    
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        // Only handle menu navigation at window level when in menu mode
        if (_inMenuMode)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (_selectedMenuIndex > 0)
                    {
                        _selectedMenuIndex--;
                        RenderMenu();
                    }
                    e.Handled = true;
                    break;
                    
                case Key.Down:
                    if (_selectedMenuIndex < _currentMenuItems.Count - 1)
                    {
                        _selectedMenuIndex++;
                        RenderMenu();
                    }
                    e.Handled = true;
                    break;
                    
                case Key.Enter:
                    ExecuteMenuItem(_currentMenuItems[_selectedMenuIndex]);
                    e.Handled = true;
                    break;
                    
                case Key.Escape:
                    ExitMenuMode();
                    e.Handled = true;
                    break;
            }
        }
    }

    private void OnCommandInputKeyDown(object? sender, KeyEventArgs e)
    {
        // Menu mode is handled at window level now
        if (_inMenuMode)
        {
            return;
        }
        
        // Normal command mode
        switch (e.Key)
        {
            case Key.Enter:
            {
                var command = CommandInput.Text?.Trim() ?? string.Empty;
                
                if (!string.IsNullOrWhiteSpace(command))
                {
                    // Echo command to terminal
                    WriteToTerminal($"> {command}");

                    // Add to history
                    _commandHistory.Add(command);
                    _historyIndex = _commandHistory.Count;

                    // Process command
                    ProcessCommand(command);

                    // Clear input
                    CommandInput.Text = string.Empty;
                }

                e.Handled = true;
                break;
            }
            case Key.Up:
            {
                // Navigate command history up
                if (_commandHistory.Count > 0 && _historyIndex > 0)
                {
                    _historyIndex--;
                    CommandInput.Text = _commandHistory[_historyIndex];
                    CommandInput.CaretIndex = CommandInput.Text.Length;
                }

                e.Handled = true;
                break;
            }
            case Key.Down:
            {
                // Navigate command history down
                if (_historyIndex < _commandHistory.Count - 1)
                {
                    _historyIndex++;
                    CommandInput.Text = _commandHistory[_historyIndex];
                    CommandInput.CaretIndex = CommandInput.Text.Length;
                }
                else if (_historyIndex == _commandHistory.Count - 1)
                {
                    _historyIndex = _commandHistory.Count;
                    CommandInput.Text = string.Empty;
                }

                e.Handled = true;
                break;
            }
        }
    }

    private void OnCommandInputLostFocus(object? sender, RoutedEventArgs e)
    {
        // Immediately refocus the command input if it loses focus
        CommandInput.Focus();
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        // Set focus after window is fully rendered
        CommandInput.Focus();
    }

    private void ProcessCommand(string command)
    {
        var parts = command.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return;
        }

        switch (parts[0])
        {
            case "help":
                ShowHelp();
                break;

            case "clear":
            case "cls":
                ClearTerminal();
                break;

            case "echo":
                if (parts.Length > 1) WriteToTerminal(string.Join(" ", parts.Skip(1)));
                break;

            case "color":
                if (parts.Length > 1)
                    ChangeColor(parts[1]);
                else
                    WriteToTerminal("Usage: color <green|amber|white|cyan>");
                break;

            case "rps":
                if (parts.Length > 1)
                    PlayRockPaperScissors(parts[1]);
                else
                    ShowRpsHelp();
                break;
            
            case "rpsstats":
                ShowRpsStats();
                break;
            
            case "menu":
                ShowDemoMenu();
                break;

            case "exit":
            case "quit":
                Close();
                break;

            default:
                WriteToTerminal($"Unknown command: {parts[0]}");
                WriteToTerminal("Type 'help' for available commands.");
                break;
        }

        WriteToTerminal("");
    }

    private void ShowHelp()
    {
        WriteToTerminal("Available commands:");
        WriteToTerminal("  help          - Show this help message");
        WriteToTerminal("  clear / cls   - Clear the terminal screen");
        WriteToTerminal("  echo <text>   - Echo text to the terminal");
        WriteToTerminal("  color <color> - Change terminal text color (green|amber|white|cyan)");
        WriteToTerminal("  rps <choice>  - Play Rock Paper Scissors (rock/paper/scissors or r/p/s)");
        WriteToTerminal("  rpsstats      - Show your RPS win/loss record");
        WriteToTerminal("  menu          - Show demo menu (navigate with ↑↓, select with Enter)");
        WriteToTerminal("  exit / quit   - Exit the application");
    }
    
    private void ShowRpsHelp()
    {
        WriteToTerminal("Rock Paper Scissors Game");
        WriteToTerminal("Usage: rps <choice>");
        WriteToTerminal("  Choices: rock, paper, scissors (or r, p, s)");
        WriteToTerminal("  Example: rps rock");
        WriteToTerminal("");
        WriteToTerminal("Check your stats with: rpsstats");
    }
    
    private void PlayRockPaperScissors(string playerChoice)
    {
        // Normalize player choice
        var choice = playerChoice.ToLower() switch
        {
            "r" or "rock" => "rock",
            "p" or "paper" => "paper",
            "s" or "scissors" => "scissors",
            _ => null
        };
        
        if (choice == null)
        {
            WriteToTerminal("Invalid choice! Use: rock, paper, scissors (or r, p, s)");
            return;
        }
        
        // Computer makes random choice
        var choices = new[] { "rock", "paper", "scissors" };
        var computerChoice = choices[_random.Next(choices.Length)];
        
        WriteToTerminal($"You chose: {choice}");
        WriteToTerminal($"Computer chose: {computerChoice}");
        WriteToTerminal("");
        
        // Determine winner
        if (choice == computerChoice)
        {
            WriteToTerminal("It's a TIE!");
            _rpsTies++;
        }
        else if (
            (choice == "rock" && computerChoice == "scissors") ||
            (choice == "paper" && computerChoice == "rock") ||
            (choice == "scissors" && computerChoice == "paper"))
        {
            WriteToTerminal("★ YOU WIN! ★");
            _rpsWins++;
        }
        else
        {
            WriteToTerminal("You LOSE!");
            _rpsLosses++;
        }
        
        WriteToTerminal($"Record: {_rpsWins}W - {_rpsLosses}L - {_rpsTies}T");
    }
    
    private void ShowRpsStats()
    {
        WriteToTerminal("═══════════════════════════════════");
        WriteToTerminal("  ROCK PAPER SCISSORS STATISTICS");
        WriteToTerminal("═══════════════════════════════════");
        WriteToTerminal($"  Wins:   {_rpsWins}");
        WriteToTerminal($"  Losses: {_rpsLosses}");
        WriteToTerminal($"  Ties:   {_rpsTies}");
        
        var totalGames = _rpsWins + _rpsLosses + _rpsTies;
        if (totalGames > 0)
        {
            var winRate = (double)_rpsWins / totalGames * 100;
            WriteToTerminal($"  Total:  {totalGames} games");
            WriteToTerminal($"  Win %:  {winRate:F1}%");
        }
        else
        {
            WriteToTerminal("  No games played yet!");
        }
        WriteToTerminal("═══════════════════════════════════");
    }

    private void WriteToTerminal(string text)
    {
        _terminalText.AppendLine(text);
        TerminalOutput.Text = _terminalText.ToString();

        // Auto-scroll to bottom
        OutputScroller.ScrollToEnd();
    }
    
    // ===== Menu System =====
    
    private void ShowMenu(string title, List<MenuItem> items)
    {
        // Clear terminal before showing menu
        ClearTerminal();
        
        _inMenuMode = true;
        _currentMenuItems = items;
        _selectedMenuIndex = 0;
        CommandInput.IsReadOnly = true;
        CommandInput.Text = "";
        
        // Hide the command input area
        SeparatorBorder.IsVisible = false;
        
        WriteToTerminal($"╔══════════════════════════════════════════════════════════╗");
        WriteToTerminal($"  {title}");
        WriteToTerminal($"╚══════════════════════════════════════════════════════════╝");
        WriteToTerminal("");
        
        // Store the position where menu items start
        _menuContentStartLength = _terminalText.Length;
        
        RenderMenu();
    }
    
    private void RenderMenu()
    {
        // Remove everything after the menu content start position
        _terminalText.Length = _menuContentStartLength;
        
        // Render menu items
        for (int i = 0; i < _currentMenuItems.Count; i++)
        {
            var item = _currentMenuItems[i];
            var prefix = i == _selectedMenuIndex ? "► " : "  ";
            var line = $"{prefix}{item.Label}";
            
            _terminalText.AppendLine(line);
        }
        
        _terminalText.AppendLine("");
        _terminalText.AppendLine("Use ↑↓ arrows to navigate, Enter to select, Esc to exit");
        
        TerminalOutput.Text = _terminalText.ToString();
    }
    
    private void ExecuteMenuItem(MenuItem item)
    {
        ExitMenuMode();
        WriteToTerminal($"> Selected: {item.Label}");
        WriteToTerminal("");
        item.Action?.Invoke();
        WriteToTerminal("");
    }
    
    private void ExitMenuMode()
    {
        _inMenuMode = false;
        _currentMenuItems.Clear();
        _selectedMenuIndex = 0;
        CommandInput.IsReadOnly = false;
        CommandInput.Text = "";
        
        // Show the command input area again
        SeparatorBorder.IsVisible = true;
    }
    
    private void ShowDemoMenu()
    {
        var menuItems = new List<MenuItem>
        {
            new MenuItem("Play Rock Paper Scissors", () => WriteToTerminal("Type 'rps rock', 'rps paper', or 'rps scissors' to play!")),
            new MenuItem("View RPS Statistics", ShowRpsStats),
            new MenuItem("Change Color to Green", () => ChangeColor("green")),
            new MenuItem("Change Color to Amber", () => ChangeColor("amber")),
            new MenuItem("Change Color to Cyan", () => ChangeColor("cyan")),
            new MenuItem("Clear Terminal", ClearTerminal),
            new MenuItem("Show Help", ShowHelp),
            new MenuItem("Exit Menu", () => WriteToTerminal("Menu closed"))
        };
        
        ShowMenu("MAIN MENU", menuItems);
    }
}

// Helper class for menu items
public class MenuItem
{
    public string Label { get; set; }
    public Action? Action { get; set; }
    
    public MenuItem(string label, Action? action = null)
    {
        Label = label;
        Action = action;
    }
}