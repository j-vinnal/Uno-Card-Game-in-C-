using System.Text;
using ConsoleApp;
using DAL;
using Domain;
using GameEngine;
using Microsoft.EntityFrameworkCore;
using UnoConsoleUI;

Console.OutputEncoding = Encoding.UTF8;


var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
    // .UseLoggerFactory(MyLoggerFactory)
    .UseSqlite("Data Source=C:/Projects/Uni/icd0008-23f/Uno/uno.db")
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
    .Options;

using var db = new AppDbContext(contextOptions);

db.Database.Migrate();

var gameOptions = new GameOptions();
var simulationOptions = new SimulationOptions();

//IGameRepository gameRepository = new GameRepositoryFileSystem();
IGameRepository gameRepository = new GameRepositoryEF(db);


var mainMenu = ProgramMenus.GetMainMenu(
    gameOptions,
    ProgramMenus.GetOptionsMenu(gameOptions),
    ProgramMenus.GetSimulationOptionsMenu(simulationOptions, SimulateGames),
    NewGame,
    LoadGame,
    SimulateGames
);


// ================== MAIN =====================
mainMenu.Run();



// ================ THE END ==================
return;


// ================== NEW GAME =====================
string? NewGame()
{
    // game logic, shared between console and web
    var gameEngine = new UnoGameEngine(gameOptions);

    // set up players
    PlayerSetup.ConfigurePlayers(gameEngine);

    // set up the table
    gameEngine.ShuffleAndDealCards();

    // console controller for game loop
    var gameController = new GameController(gameEngine, gameRepository);


    gameController.Run();


    return null;
}

// ================== SIMULATE GAMES =====================
string? SimulateGames()
{

    // game logic, shared between console and web
    var gameEngine = new UnoGameEngine(gameOptions);

    // set up players
    PlayerSetup.ConfigurePlayers(gameEngine);
    var players = gameEngine.State.Players;
    Console.Clear();

    Console.WriteLine(
        $"Simulating {simulationOptions.SimulationCount} games - one simulation may take a few seconds, depending on the number of players");
    Console.WriteLine("AI players are generally stronger but slower");
    Console.WriteLine();
    var winnersList = new List<Player>();
    for (int i = 1; i <= simulationOptions.SimulationCount; i++)
    {
        // set up the table
        gameEngine.ShuffleAndDealCards();

        while (!gameEngine.IsGameOver())
        {
            if (gameEngine.GetActivePlayer().PlayerType == EPlayerType.AI)
            {
                gameEngine.AiTurn();
            }
            else
            {
                gameEngine.AiTurn(true);
            }
        }

        var winner = gameEngine.GetWinner();
        winnersList.Add(winner!);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Game no {i} Winner is: {winner!.PlayerType} {winner!.NickName}");
        Console.ResetColor();
        foreach (var player in gameEngine.State.Players)
        {
            Console.WriteLine(
                $"\u001b[2;90m{player.PlayerType}\u001b[0m {player.NickName} card count: {player.PlayerHand.Count}");
        }

        Console.WriteLine();
    }

    Console.WriteLine();
    Console.WriteLine("Simulation statistics:");

    var totalGames = simulationOptions.SimulationCount;
    var playerStatistics = winnersList.GroupBy(p => p.Id)
        .Select(group => new
        {
            Id = group.Key,
            NickName =  group.First().NickName,
            PlayerType = group.First().PlayerType,
            Wins = group.Count(),
            WinPercentage = (double)group.Count() / totalGames * 100
        })
        .OrderByDescending(p => p.Wins)
        .ToList();
    
    var mostFrequentWinner = winnersList.GroupBy(p => p.Id)
        .Select(group => new
        {
            Id = group.Key,
            NickName =  group.First().NickName,
            PlayerType = group.First().PlayerType,
            Wins = group.Count(),
            WinPercentage = (double)group.Count() / totalGames * 100
        })
        .OrderByDescending(p => p.Wins)
        .First();


    foreach (var playerStat in playerStatistics)
    {
        Console.WriteLine(
            $"{playerStat.NickName} ({playerStat.PlayerType}): {playerStat.Wins} wins ({playerStat.WinPercentage:F2}%)");
    }

    Console.WriteLine();
    Console.WriteLine("Graphical representation:");

 
    foreach (var player in players)
    {
        if (player.Id == mostFrequentWinner!.Id)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        }
        else
        {
            Console.ResetColor();
        }
        Console.Write($"{player.NickName,-12}|");
        for (int i = 1; i <= simulationOptions.SimulationCount; i++)
        {
            char resultChar = winnersList[i - 1].Id == player.Id ? 'X' : ' ';
            Console.Write($"{resultChar, 4}   ");
        }
        Console.WriteLine();
        
        Console.ResetColor();
    }
    
    Console.WriteLine(new string('-', (4 + 3) * simulationOptions.SimulationCount + 12));
    
    Console.Write("Game        |");
    for (int i = 1; i <= simulationOptions.SimulationCount; i++)
    {
        Console.Write($"   {i,-4}");
    }

    Console.WriteLine();
    Console.ReadLine();
    return null;
 
}

// ================== LOAD GAME =====================
string? LoadGame()
{
    Console.WriteLine("Saved games");
    var saveGameList = gameRepository.GetSaveGames();
    var saveGameListDisplay = saveGameList.Select((s, i) => (i + 1) + " - " + s).ToList();

    if (saveGameListDisplay.Count == 0) return null;

    Guid gameId;
    while (true)
    {
        Console.WriteLine(string.Join("\n", saveGameListDisplay));
        Console.Write($"Select game to load (1..{saveGameListDisplay.Count}):");
        var userChoiceStr = Console.ReadLine();
        if (int.TryParse(userChoiceStr, out var userChoice))
        {
            if (userChoice < 1 || userChoice > saveGameListDisplay.Count)
            {
                Console.WriteLine("Not in range...");
                continue;
            }

            gameId = saveGameList[userChoice - 1].id;
            Console.WriteLine($"Loading file: {gameId}");

            break;
        }

        Console.WriteLine("Parse error...");
    }


    var gameState = gameRepository.LoadGame(gameId);

    var gameEngine = new UnoGameEngine(gameOptions)
    {
        State = gameState
    };

    var gameController = new GameController(gameEngine, gameRepository);

    gameController.Run();

    return null;
}