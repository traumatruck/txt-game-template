using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using TxtGameTemplate.App.Commands;
using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App;

public partial class MainWindow : Window
{
    // Services
    private readonly TerminalService _terminalService;
    private readonly CommandRegistry _commandRegistry;
    
    // UI State
    private readonly List<string> _commandHistory = [];
    private int _historyIndex = -1;
    
    // Menu system state
    private bool _inMenuMode = false;
    private List<MenuItem> _currentMenuItems = [];
    private int _selectedMenuIndex = 0;
    private int _menuContentStartLength = 0;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        _terminalService = new TerminalService(TerminalOutput, OutputScroller);
        _commandRegistry = new CommandRegistry();
        
        // Register built-in commands
        RegisterCommands();
        
        InitializeTerminal();

        // Ensure focus is set after window is fully loaded
        Opened += OnWindowOpened;
    }
    
    private void RegisterCommands()
    {
        // System commands
        _commandRegistry.Register(new ClearCommand(_terminalService));
        _commandRegistry.Register(new HelpCommand(_terminalService, _commandRegistry));
        _commandRegistry.Register(new EchoCommand(_terminalService));
        
        // Game commands
        _commandRegistry.Register(new RpsCommand(_terminalService));
        
        // UI commands
        _commandRegistry.Register(new ColorCommand(this, _terminalService));
        _commandRegistry.Register(new MenuCommand(this));
    }

    public void ChangeColor(string color)
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
            
            _terminalService.WriteLine($"Color changed to {color}");
        }
        else
        {
            _terminalService.WriteLine($"Unknown color: {color}");
            _terminalService.WriteLine("Available colors: green, amber, white, cyan");
        }
    }

    private void ClearTerminal()
    {
        _terminalService.Clear();
    }

    private void InitializeTerminal()
    {
        // Display welcome message
        _terminalService.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        _terminalService.WriteLine("║          TEXT GAME FRAMEWORK v1.0                         ║");
        _terminalService.WriteLine("║          Retro Style Terminal                             ║");
        _terminalService.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        _terminalService.WriteLine("");
        _terminalService.WriteLine("Type 'help' for available commands.");
        _terminalService.WriteLine("");

        // Set up event handlers
        CommandInput.KeyDown += OnCommandInputKeyDown;
        CommandInput.LostFocus += OnCommandInputLostFocus;
        
        // Handle keyboard input at window level for menu navigation
        KeyDown += OnWindowKeyDown;

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
                    _terminalService.WriteLine($"> {command}");

                    // Add to history
                    _commandHistory.Add(command);
                    _historyIndex = _commandHistory.Count;

                    // Process command through registry
                    if (!_commandRegistry.TryExecute(command))
                    {
                        _terminalService.WriteLine($"Unknown command: {command.Split(' ')[0]}");
                        _terminalService.WriteLine("Type 'help' for available commands.");
                    }
                    
                    _terminalService.WriteLine("");

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

    // All command logic has been moved to Commands/ folder
    // Commands are now handled through CommandRegistry
    
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
        
        _terminalService.WriteLine($"╔══════════════════════════════════════════════════════════╗");
        _terminalService.WriteLine($"  {title}");
        _terminalService.WriteLine($"╚══════════════════════════════════════════════════════════╝");
        _terminalService.WriteLine("");
        
        // Store the position where menu items start
        _menuContentStartLength = _terminalService.GetTextLength();
        
        RenderMenu();
    }
    
    private void RenderMenu()
    {
        // Remove everything after the menu content start position
        _terminalService.SetTextLength(_menuContentStartLength);
        
        // Render menu items
        for (var i = 0; i < _currentMenuItems.Count; i++)
        {
            var item = _currentMenuItems[i];
            var prefix = i == _selectedMenuIndex ? "► " : "  ";
            var line = $"{prefix}{item.Label}";
            
            _terminalService.WriteLine(line);
        }
        
        _terminalService.WriteLine("");
        _terminalService.WriteLine("Use ↑↓ arrows to navigate, Enter to select, Esc to exit");
    }
    
    private void ExecuteMenuItem(MenuItem item)
    {
        ExitMenuMode();
        _terminalService.WriteLine($"> Selected: {item.Label}");
        _terminalService.WriteLine("");
        item.Action?.Invoke();
        _terminalService.WriteLine("");
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
        
        // Restore focus to command input
        CommandInput.Focus();
    }
    
    public void ShowDemoMenu()
    {
        var menuItems = new List<MenuItem>
        {
            new("Play Rock Paper Scissors", () => _terminalService.WriteLine("Type 'rps rock', 'rps paper', or 'rps scissors' to play!")),
            new("View RPS Statistics", () => _commandRegistry.TryExecute("rps stats")),
            new("Change Color to Green", () => ChangeColor("green")),
            new("Change Color to Amber", () => ChangeColor("amber")),
            new("Change Color to Cyan", () => ChangeColor("cyan")),
            new("Clear Terminal", ClearTerminal),
            new("Show Help", () => _commandRegistry.TryExecute("help")),
            new("Exit Menu", () => _terminalService.WriteLine("Menu closed"))
        };
        
        ShowMenu("MAIN MENU", menuItems);
    }
}

// Helper class for menu items
public class MenuItem(string label, Action? action = null)
{
    public string Label { get; set; } = label;

    public Action? Action { get; set; } = action;
}