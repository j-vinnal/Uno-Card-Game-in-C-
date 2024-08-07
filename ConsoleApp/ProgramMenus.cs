using Domain;
using MenuSystem;

namespace ConsoleApp;

public static class ProgramMenus
{
    
    private static string label = "     \u001b[31m >> \u001b[34mU\u001b[32mN\u001b[33mO\u001b[31m << \u001b[0m";
    public static Menu GetOptionsMenu(GameOptions gameOptions) =>
        new Menu("Options", new List<MenuItem>()
        {
            new MenuItem()
            {
                Shortcut = "h",
                MenuLabelFunction = () => "Hand size - " + gameOptions.HandSize,
                MethodToRun = () => OptionsChanger.ConfigureHandSize(gameOptions)
            },
            new MenuItem()
            {
                Shortcut = "ai",
                MenuLabelFunction = () => "AI move speed in sek - " + gameOptions.AiSpeed / 1000,
                MethodToRun = () => OptionsChanger.ConfigureAiSpeed(gameOptions)
            }
        });
    
    public static Menu GetSimulationOptionsMenu(SimulationOptions simulationOptions, Func<string?> simulateGamesMethod) =>
        new Menu("Simulation options", new List<MenuItem>()
        {
            new MenuItem()
            {
                Shortcut = "s",
                MenuLabelFunction = () => $"Start simulation \u001b[2;90m{simulationOptions}\u001b[0m",
                MethodToRun = simulateGamesMethod
            },
            new MenuItem()
            {
            Shortcut = "o",
            MenuLabelFunction = () => $"Options",
            MethodToRun = () => OptionsChanger.ConfigureSimulationCount(simulationOptions)
        }
        });
    
    
    public static Menu GetMainMenu(GameOptions gameOptions, Menu optionsMenu, Menu simulationOptionMenu, Func<string?> newGameMethod, Func<string?> loadGameMethod, Func<string?> simulateGamesMethod) => 
        new Menu(label, new List<MenuItem>()
        {
            new MenuItem()
            {
                Shortcut = "s",
                MenuLabel = "Start a new game: ",
                MenuLabelFunction = () =>  $"Start a new game \u001b[2;90m{gameOptions}\u001b[0m",
                MethodToRun = newGameMethod
            },
            new MenuItem()
            {
                Shortcut = "l",
                MenuLabel = "Load game",
                MethodToRun = loadGameMethod
            },
            new MenuItem()
            {
                Shortcut = "o",
                MenuLabel = "Options",
                MethodToRun = () => optionsMenu.Run(EMenuLevel.Second)
            },
            new MenuItem()
            {
                Shortcut = "a",
                MenuLabel = $"Simulate games",
                MethodToRun = () =>  simulationOptionMenu.Run(EMenuLevel.Second)
            },
        });
}