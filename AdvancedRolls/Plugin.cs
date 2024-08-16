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
    //private PluginUI ui;

    
    public Plugin(IDalamudPluginInterface pluginInterface, IChatGui chat, IPartyList partyList, ICommandManager commands)
    {
        PartyList = partyList;
        Chat = chat;

        PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        PluginConfig.Initialize(PluginInterface);

        /*
        ui = new PluginUI();
        PluginInterface.UiBuilder.Draw += new System.Action(ui.Draw);
        PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            PluginUI ui = this.ui;
            ui.IsVisible = !ui.IsVisible;
        };
        */

        CommandManager = new PluginCommandManager<Plugin>(this, commands);
    }

   

    [Command("/advancedroll")]
    [Aliases("/ar")]
    [HelpMessage("Roll an advanced dice. You can do a min/max roll (Ex: /ar 20 120) or a custom roll, including modifier (Ex: /ar 1d20+5)")]
    public void Roll(string command, string args)
    {
        // Verify if its DnD arguments
        if (TryParseDiceExpression(args, out int result))
        {
            Chat.Print($"Result of the roll {args}: {result}");
            return;
        }

        // If not, roll with min/max logic
        var splitArgs = args.Split(' ');
        if (splitArgs.Length != 2 ||
            !int.TryParse(splitArgs[0], out int min) ||
            !int.TryParse(splitArgs[1], out int max))
        {
            Chat.PrintError("Please enter two valid numbers or a dice expression. Ex: /ar 20 120 or /ar 1d20+5");
            return;
        }

        // Valider les nombres
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
            Chat.PrintError("The numbers must be lower or equal than 999. Ex: /ar 20 120");
            return;
        }

        // Générer un nombre aléatoire
        var random = new Random();
        int rollResult = random.Next(min, max + 1);

        Chat.Print($"Result of the roll between {min} and {max}: {rollResult}");
    }

    private bool TryParseDiceExpression(string input, out int result)
    {
        result = 0;
        var random = new Random();

        var match = System.Text.RegularExpressions.Regex.Match(input, @"(\d+)d(\d+)([+-]\d+)?");

        if (match.Success)
        {
            int numberOfDice = int.Parse(match.Groups[1].Value);
            int diceSides = int.Parse(match.Groups[2].Value);
            int modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;


            for (int i = 0; i < numberOfDice; i++)
            {
                result += random.Next(1, diceSides + 1);
            }

            result += modifier;

            return true;
        }

        return false;
    }

    /*
    [Command("/config")]
    [HelpMessage("Jeter un dé avec une valeur minimal et une valeur maximale. Ex: /roll 20 120")]
    public void Config(string command, string args)
    {
        ui.IsVisible = !ui.IsVisible;
    }
    */

    public void Dispose()
    {
        CommandManager.Dispose();

        PluginInterface.SavePluginConfig(PluginConfig);

        /*
        PluginInterface.UiBuilder.Draw -= ui.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= () =>
        {
            PluginUI ui = this.ui;
            ui.IsVisible = !ui.IsVisible;
        };
        */
    }

}
