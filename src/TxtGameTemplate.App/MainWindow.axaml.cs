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

        // Prevent focus from leaving the command input
        TerminalOutput.Focusable = false;
        TerminalOutput.IsReadOnly = true;
    }

    private void OnCommandInputKeyDown(object? sender, KeyEventArgs e)
    {
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
        WriteToTerminal("  exit / quit   - Exit the application");
    }

    private void WriteToTerminal(string text)
    {
        _terminalText.AppendLine(text);
        TerminalOutput.Text = _terminalText.ToString();

        // Auto-scroll to bottom
        OutputScroller.ScrollToEnd();
    }
}