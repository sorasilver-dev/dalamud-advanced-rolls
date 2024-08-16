using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System;
using AdvancedRolls.Other;


namespace AdvancedRolls;

public class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface? PluginInterface { get; set; }
    [PluginService] public static IChatGui? Chat { get; set; }
    [PluginService] public static IPartyList? PartyList { get; set; }
    
    public static Configuration? PluginConfig { get; set; }

    private PluginCommandManager<Plugin> CommandManager;
    
    public Plugin(IDalamudPluginInterface pluginInterface, IChatGui chat, IPartyList partyList, ICommandManager commands)
    {
        PartyList = partyList;
        Chat = chat;

        PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        PluginConfig.Initialize(PluginInterface);

        CommandManager = new PluginCommandManager<Plugin>(this, commands);
    }

   

    [Command("/advancedroll")]
    [Aliases("/ar")]
    [HelpMessage("Roll an advanced dice. You can do a min/max roll (Ex: /ar 20 120) or a custom roll, including modifier (Ex: /ar 1d20+5)")]
    public void Roll(string command, string args)
    {
        if (IsDiceExpression(args))
        {
            // DnD Roll
            HandleDiceRoll(args);
        }
        else if(IsMinMaxExpression(args))
        {
            // Min/Max Roll
            HandleMinMaxRoll(args);
        }
        else
        {
            Chat.PrintError("Invalid format. Ex: /ar 20 120 or /ar 1d20");
            return;
        }
    }
    private void HandleMinMaxRoll(string args)
    {
        var splitArgs = args.Split(' ');
        if (splitArgs.Length != 2 ||
            !int.TryParse(splitArgs[0], out int min) ||
            !int.TryParse(splitArgs[1], out int max))
        {
            Chat.PrintError("Please enter two valid numbers. Ex: /ar 20 120");
            return;
        }

        if (min >= max)
        {
            Chat.PrintError("The first number must be lower than the second number. Ex: /ar 20 120");
            return;
        }

        if (min < 0 || max < 0)
        {
            Chat.PrintError("The numbers must be positive. Ex: /ar 20 120");
            return;
        }

        if (min > 999 || max > 999)
        {
            Chat.PrintError("The numbers must be lower or equal to 999. Ex: /ar 20 120");
            return;
        }

        var random = new Random();
        int rollResult = random.Next(min, max + 1);

        Chat.Print($"Result of the roll between {min} and {max}: {rollResult}");
    }

    private bool IsDiceExpression(string input)
    {
        // Regex to check if its a DnD expression
        return System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d+d\d+([+-]\d+)?$");
    }private bool IsMinMaxExpression(string input)
    {
        // Regex to check if its a DnD expression
        return System.Text.RegularExpressions.Regex.IsMatch(input, @"^(\d+)\s+(\d+)$");
    }
    private void HandleDiceRoll(string args)
    {
        var match = System.Text.RegularExpressions.Regex.Match(args, @"^(\d+)d(\d+)([+-]\d+)?$");

        var result = 0;
        var random = new Random();
        int numberOfDice = int.Parse(match.Groups[1].Value);
        int diceSides = int.Parse(match.Groups[2].Value);
        int modifier = 0;

        if (match.Groups[3].Success)
        {
            modifier = int.Parse(match.Groups[3].Value);
        }


        if (numberOfDice > 100)
        {
            Chat.PrintError("The number of dices must be lower or equal to 100. Ex: /ar 1d20");
            return;
        }

        if (diceSides > 999)
        {
            Chat.PrintError("The number of sides must be lower or equal to 999. Ex: /ar 1d20");
            return;
        }

        if (modifier > 999)
        {
            Chat.PrintError("The modifier must be lower or equal to 999. Ex: /ar 1d20+5");
            return;
        }

        for (int i = 0; i < numberOfDice; i++)
        {
            result += random.Next(1, diceSides + 1);
        }

        result += modifier;

        Chat.Print($"Result of the roll {args}: {result}");

    }

    public void Dispose()
    {
        CommandManager.Dispose();

        PluginInterface.SavePluginConfig(PluginConfig);
    }

}
