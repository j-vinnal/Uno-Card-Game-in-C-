using Domain;
using GameEngine;

namespace ConsoleApp
{
    public class PlayerSetup
    {
        public static void ConfigurePlayers(UnoGameEngine gameEngine)
        {
            var playerCount = 0;

            while (true)
            {
                Console.Write("How many players (2 - 10)[3]:");
                var playerCountStr = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(playerCountStr)) playerCountStr = "3";
                if (int.TryParse(playerCountStr, out playerCount))
                {
                    if (playerCount is >= 2 and <= 10) break;
                    Console.WriteLine("Number not in range...");
                }
            }

            for (int i = 1; i <= playerCount; i++)
            {
                
                string? typeStr = "";
                while (true)
                {
                    Console.Write($"Player {i + 1} type (A - AI / H - Human / R - Random)[{(i % 3 == 0 ? "H" : (i % 3 == 1 ? "A" : "R"))}]:");
                    typeStr = Console.ReadLine()?.ToUpper().Trim();
                    if (string.IsNullOrWhiteSpace(typeStr))
                    {
                        typeStr = i % 3 == 0 ? "H" : (i % 3 == 1 ? "A" : "R");
                    }

                    if (typeStr == "A" || typeStr == "H" || typeStr == "R") break;
                    Console.WriteLine("Invalid player type. Please enter 'A' for AI, 'H' for Human, or 'R' for Random.");
                }
                
                EPlayerType playerType = (typeStr) switch
                {
                    "H" => EPlayerType.Human,
                    "A" => EPlayerType.AI,
                    "R" => EPlayerType.Random,
                    _ => throw new InvalidOperationException("Unexpected player type index"),
                };

                string? playerName = "";
                while (true)
                {
                    Console.Write($"\u001b[2;90m{playerType}\u001b[0m Player {i} name (min 1 letter)[{playerType.ToString().ToLower()[0]}{i}]:");
                    playerName = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(playerName))
                    {
                        playerName = playerType.ToString().ToLower()[0] + i.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(playerName) && playerName.Length > 0) break;
                    Console.WriteLine("Parse error...");
                }

                gameEngine.State.Players.Add(new Player()
                {
                    NickName = playerName,
                    PlayerType = playerType,
                });
            }
        }
    }
}