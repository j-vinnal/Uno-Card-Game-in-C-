using Domain;

namespace ConsoleApp;

public class OptionsChanger
{
    public static string? ConfigureHandSize(GameOptions gameOptions)
    {
        while (true)
        {
            Console.Write($"Enter hand size:");
            var sizeStr = Console.ReadLine();

            if (sizeStr == null) continue;

            if (!int.TryParse(sizeStr, out var size))
            {
                Console.WriteLine("Parse error...");
                continue;
            }

            if (size < 2)
            {
                Console.WriteLine("Out of range...");
                continue;
            }


            gameOptions.HandSize = size;
            return null;
        }
    }
    
    
    public static string? ConfigureAiSpeed(GameOptions gameOptions)
    {
        while (true)
        {
            Console.Write($"Enter AI move speed in sek:");
            var sizeStr = Console.ReadLine();

            if (sizeStr == null) continue;

            if (!int.TryParse(sizeStr, out var size))
            {
                Console.WriteLine("Parse error...");
                continue;
            }

            if (size < 0)
            {
                Console.WriteLine("Out of range...");
                continue;
            }


            gameOptions.AiSpeed = size * 1000;
            return null;
        }
    }
    
    public static string? ConfigureSimulationCount(SimulationOptions simulatonOptions)
    {
        while (true)
        {
            Console.Write($"Enter the number of simulations:");
            var sizeStr = Console.ReadLine();

            if (sizeStr == null) continue;

            if (!int.TryParse(sizeStr, out var size))
            {
                Console.WriteLine("Parse error...");
                continue;
            }

            if (size < 0)
            {
                Console.WriteLine("Out of range...");
                continue;
            }
            
            simulatonOptions.SimulationCount= size;
            return null;
        }
    }

    
}