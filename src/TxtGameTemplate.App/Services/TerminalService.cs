using System.Text;
using Avalonia.Controls;

namespace TxtGameTemplate.App.Services;

/// <summary>
///     Terminal service implementation that manages terminal output
/// </summary>
public class TerminalService(TextBox outputTextBox, ScrollViewer scrollViewer) : ITerminalService
{
    private readonly StringBuilder _terminalText = new();

    public void Clear()
    {
        _terminalText.Clear();
        outputTextBox.Text = string.Empty;
    }

    /// <summary>
    ///     Get current terminal text length (for menu system)
    /// </summary>
    public int GetTextLength()
    {
        return _terminalText.Length;
    }

    /// <summary>
    ///     Set terminal text length (for menu system truncation)
    /// </summary>
    public void SetTextLength(int length)
    {
        _terminalText.Length = length;
        outputTextBox.Text = _terminalText.ToString();
    }

    public void WriteLine(string text)
    {
        _terminalText.AppendLine(text);
        outputTextBox.Text = _terminalText.ToString();
        scrollViewer.ScrollToEnd();
    }
}