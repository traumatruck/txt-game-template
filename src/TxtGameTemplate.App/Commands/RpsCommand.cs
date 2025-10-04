using System;
using TxtGameTemplate.App.Services;

namespace TxtGameTemplate.App.Commands;

/// <summary>
///     Rock Paper Scissors game command
/// </summary>
public class RpsCommand(ITerminalService terminal) : ICommand
{
    private readonly Random _random = new();
    
    private int _losses;
    private int _ties;
    private int _wins;

    public string[] Aliases => [];

    public string Name => "rps";
    
    public string Description => "Play Rock Paper Scissors (usage: rps rock/paper/scissors, rps stats)";

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        if (args[0].Equals("stats", StringComparison.CurrentCultureIgnoreCase))
        {
            ShowStats();
            return;
        }

        PlayGame(args[0]);
    }

    private void PlayGame(string playerChoice)
    {
        var choice = playerChoice.ToLower() switch
        {
            "r" or "rock" => "rock",
            "p" or "paper" => "paper",
            "s" or "scissors" => "scissors",
            _ => null
        };

        if (choice == null)
        {
            terminal.WriteLine("Invalid choice! Use: rock, paper, scissors (or r, p, s)");
            return;
        }

        var choices = new[] { "rock", "paper", "scissors" };
        var computerChoice = choices[_random.Next(choices.Length)];

        terminal.WriteLine($"You chose: {choice}");
        terminal.WriteLine($"Computer chose: {computerChoice}");
        terminal.WriteLine("");

        if (choice == computerChoice)
        {
            terminal.WriteLine("It's a TIE!");
            _ties++;
        }
        else if (
            (choice == "rock" && computerChoice == "scissors") ||
            (choice == "paper" && computerChoice == "rock") ||
            (choice == "scissors" && computerChoice == "paper"))
        {
            terminal.WriteLine("★ YOU WIN! ★");
            _wins++;
        }
        else
        {
            terminal.WriteLine("You LOSE!");
            _losses++;
        }

        terminal.WriteLine($"Record: {_wins}W - {_losses}L - {_ties}T");
    }

    private void ShowHelp()
    {
        terminal.WriteLine("Rock Paper Scissors Game");
        terminal.WriteLine("Usage: rps <choice>");
        terminal.WriteLine("  Choices: rock, paper, scissors (or r, p, s)");
        terminal.WriteLine("  Example: rps rock");
        terminal.WriteLine("");
        terminal.WriteLine("Check your stats with: rps stats");
    }

    private void ShowStats()
    {
        terminal.WriteLine("═══════════════════════════════════");
        terminal.WriteLine("  ROCK PAPER SCISSORS STATISTICS");
        terminal.WriteLine("═══════════════════════════════════");
        terminal.WriteLine($"  Wins:   {_wins}");
        terminal.WriteLine($"  Losses: {_losses}");
        terminal.WriteLine($"  Ties:   {_ties}");

        var totalGames = _wins + _losses + _ties;
        
        if (totalGames > 0)
        {
            var winRate = (double)_wins / totalGames * 100;
            terminal.WriteLine($"  Total:  {totalGames} games");
            terminal.WriteLine($"  Win %:  {winRate:F1}%");
        }
        else
        {
            terminal.WriteLine("  No games played yet!");
        }

        terminal.WriteLine("═══════════════════════════════════");
    }
}