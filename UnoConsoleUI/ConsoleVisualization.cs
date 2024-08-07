using Domain;
using Helpers;
using Player = Domain.Player;

namespace UnoConsoleUI;

public static class ConsoleVisualization
{
    private static string CreateSeparator(char symbol, int length)
    {
        return new string(symbol, length);
    }

    public static void DrawPlayerInfo(GameState state)
    {
        var player = state.Players[state.ActivePlayerNo];

        Console.WriteLine(player.PlayerType == EPlayerType.AI
            ? $"\u001b[1mPlayer {state.ActivePlayerNo + 1} - [AI] {player.NickName}\u001b[0m"
            : $"\u001b[1mPlayer {state.ActivePlayerNo + 1} - {player.NickName}\u001b[0m");
        Console.WriteLine(CreateSeparator('=', 24));
    }

    public static void DrawDesk(GameState state, List<Player> playersInOrder)
    {
        var arrow = state.IsClockwise ? "\u001b[92m↓\u001b[0m" : "\u001b[91m↓\u001b[0m";
        Console.WriteLine(state.IsClockwise
            ? "Direction: \u001b[92mClockwise\u001b[0m"
            : "Direction: \u001b[91mCounterclockwise\u001b[0m");
        Console.WriteLine($"Discard pile: {state.DiscardPile.Count} cards");
        Console.WriteLine($"Draw pile: {state.DrawPile.Count} cards");

        
        for (var i = 0; i < playersInOrder.Count; i++)
        {
            var playerNumber = state.IsClockwise
                ? (state.ActivePlayerNo + i) % playersInOrder.Count + 1
                : (state.ActivePlayerNo - i + playersInOrder.Count) % playersInOrder.Count + 1;

            Console.WriteLine(
                $"{arrow} Player {playerNumber} - {playersInOrder[i].NickName}: {playersInOrder[i].PlayerHand.Count} cards");
        }

        Console.WriteLine(CreateSeparator('-', 24));
        Console.WriteLine($"Top card: {VisualizeGameCard(state.DiscardPile[^1])}");
        Console.WriteLine();
    }

   

    public static void DrawPlayerSkipTurn(Player activePlayer, Player nextPlayer, Player previousPlayer,
        GameState state, Func<GameCard, GameCard, bool> validateMove, GameCard topCard, int aiSpeed)
    {
        DrawPlayerInfo(state);
        DrawPreviousPlayerMoves(state, previousPlayer);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(
            "\u28bb\u28ad\u2853\u28c6\u2840\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \u2800\u2800\u2800\u2800\u2838\u28cf\u2896\u2872\u28c5\u2800\u2800\n\u28e3\u28be\u285b\u28dc\u28ab\u28e6\u2800\u2800\u2880\u28e4\u2834\u2866\u28c4\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28bb\u28f8\u288f\u285d\u28c6\u2880\n\u28bf\u28e7\u28b9\u28ec\u2877\u28da\u28d2\u28f6\u287e\u28cd\u285e\u2871\u28de\u2847\u2800\u2800\u2800\u2800\u2880\u28e0\u28a4\u2816\u28e6\u2864\u2824\u2876\u2826\u2824\u28e4\u28b6\u2832\u2824\u28c4\u2800\u2800\u2800\u2800\u2800\u2880\u2864\u2836\u28b6\u28a4\u2840\u28b8\u28db\u28ee\u289e\u285c\u285a\n\u2808\u2877\u28fb\u288f\u2836\u28d9\u28b6\u28fc\u281f\u287c\u28dc\u2875\u280b\u2800\u2800\u2800\u28e0\u281e\u2869\u28b4\u28ff\u28ff\u28fe\u28f9\u2810\u28a2\u2881\u287e\u2875\u281a\u28bb\u28f7\u28e4\u2859\u2832\u2884\u2800\u2800\u28be\u28cd\u287b\u28cc\u28a7\u28f7\u287e\u285e\u28e5\u28ab\u285d\u28c3\n\u2800\u28bb\u28ff\u288a\u28df\u28fe\u28ab\u2887\u287b\u28f1\u28ba\u2801\u2800\u2800\u2800\u287c\u28e1\u28ff\u28c4\u28c0\u287f\u28ff\u28ff\u284f\u2847\u28a2\u28b8\u287f\u28f7\u28e4\u28fc\u283f\u28bf\u28ff\u28f7\u28ce\u28f7\u2800\u2808\u2833\u28f5\u2869\u2896\u287b\u28f1\u28bb\u28cc\u2873\u288e\u2875\n\u2800\u2800\u28bb\u2867\u289e\u2867\u28cb\u28ee\u28d5\u2863\u28bf\u2800\u2800\u2880\u287c\u2883\u28fb\u28bf\u28ff\u28ff\u28e7\u283e\u281f\u2859\u28e7\u28c2\u28cc\u28a3\u285b\u287f\u283f\u2837\u283e\u283f\u283f\u2823\u28cc\u2833\u2840\u28b0\u28af\u2871\u28eb\u2876\u28a5\u28db\u28ae\u2853\u28cf\u28b6\n\u2800\u2800\u2808\u28af\u2867\u28d3\u28a7\u285a\u28fd\u28de\u287e\u2800\u2880\u285e\u2820\u28ff\u2800\u2870\u2882\u28d6\u28e4\u28ef\u28fe\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28c7\u2804\u28ce\u28f1\u28c9\u288e\u2871\u28d8\u2847\u2839\u285e\u28ee\u28b5\u28af\u28f1\u2833\u286c\u28a7\u2859\u28e6\u280b\n\u2800\u2800\u2800\u2808\u2833\u28ed\u28b2\u2879\u28b2\u285e\u2801\u2800\u28fc\u2890\u2821\u2859\u2833\u2817\u285b\u28e9\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ef\u2841\u2849\u281b\u28f6\u28f5\u280b\u2850\u28bf\u2808\u283b\u28c6\u28a7\u285b\u289c\u28e3\u281f\u2801\u2800\n\u2800\u2800\u2800\u2800\u2800\u2808\u2809\u2809\u2801\u2800\u2800\u28b0\u2847\u288a\u2814\u2861\u288a\u2814\u28f8\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28f7\u2848\u2821\u28ff\u28b9\u2844\u28a1\u289a\u28c7\u2800\u2808\u2809\u2809\u2809\u2800\u2800\u2800\u2800\n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u2847\u288a\u2824\u2891\u2822\u28b8\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28e7\u28b9\u2803\u28a2\u28bb\u2844\u288a\u28cf\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u28c7\u280c\u28a2\u2801\u288e\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u283f\u281f\u283f\u28bf\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28cf\u2844\u28a3\u28ba\u2847\u28bc\u284b\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28bb\u284a\u2824\u2809\u28bc\u28ff\u28ff\u28ff\u28ff\u283f\u280b\u2844\u2812\u284c\u28a2\u2810\u284c\u283b\u28ff\u28ff\u28ff\u28ff\u28ef\u281b\u2893\u281b\u28e0\u287e\u2801\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u28a7\u2858\u284f\u28ff\u28ff\u28ff\u287f\u280b\u2844\u2823\u280c\u2871\u2888\u2804\u28a3\u2810\u2861\u2818\u28bf\u28ff\u28ff\u28ff\u2850\u28cc\u2892\u28f0\u280f\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u28b3\u2845\u2838\u283f\u289b\u2861\u2818\u2844\u2823\u2858\u2804\u2823\u2858\u2804\u28a3\u2810\u28c9\u2802\u283b\u28bf\u283f\u2801\u28bc\u2872\u280b\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u28a7\u28c1\u2826\u281f\u2841\u28a3\u2810\u2861\u2802\u284d\u2830\u2881\u280e\u2844\u2823\u2884\u2849\u2832\u28a6\u28c2\u28c9\u28b4\u2817\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n \u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2809\u2819\u28b2\u28a5\u28c2\u2805\u28c2\u2811\u2848\u2885\u280a\u2850\u280c\u28a1\u2882\u28cc\u28e1\u2836\u28db\u28d9\u280b\u2800\u2800");
        Console.ResetColor();
        Console.WriteLine();
        DrawPlayerHand(activePlayer, validateMove, topCard);
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Now player {nextPlayer.NickName}'s turn..");
        Console.ResetColor();
        if (activePlayer.PlayerType == EPlayerType.AI || activePlayer.PlayerType == EPlayerType.Random)
        {
            Thread.Sleep(aiSpeed);
        }
        else
        {
            Console.ReadLine();
        }
    }


    public static void DrawForgotToSayUno()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(
            "Oops! Don't forget to say 'UNO' when you have only one card left, you'll draw two penalty cards!");
        Console.ReadLine();
        Console.ResetColor();
    }

    public static void DrawPlayerHand(Player player, Func<GameCard, GameCard, bool> validateMove, GameCard topCard,
        bool haveCardOfSameColor = false)
    {
        Console.WriteLine($"{player.NickName}'s current hand is:");

        var cardsBySuit = player.PlayerHand
            .OrderBy(card => card.CardSuit)
            .ThenBy(card => card.CardValue)
            .GroupBy(card => validateMove(card, topCard))
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var playableGroup in cardsBySuit.OrderByDescending(kv => kv.Key))
        {
            var cards = string.Join(" ", playableGroup.Value.Select(VisualizeGameCard));
            var shortcuts = string.Join(" ", playableGroup.Value.Select(card =>
                {
                    string shortcut;
                    if (card.CardValue == ECardValue.WildDrawFour && haveCardOfSameColor)
                    {
                        shortcut = $"\u001b[1;33m{card.CardShortcut}\u001b[0m";
                    }
                    else
                    {
                        shortcut = validateMove(card, topCard)
                            ? $"\u001b[1m{card.CardShortcut}\u001b[0m"
                            : $"{card.CardShortcut}";
                    }
                    
                    var cardWidth = (4 - card.CardShortcut.Length) / 2;

                    var paddingBefore = new string(' ', cardWidth);
                    var paddingAfter = new string(' ', 5 - cardWidth - card.CardShortcut.Length);

                    return $"{paddingBefore}{shortcut}{paddingAfter}";
                }
            ));

            Console.WriteLine(cards);
            Console.WriteLine(shortcuts);
        }
    }

    public static void DrawChoices(GameState state, Dictionary<string, EPlayerAction> choices)
    {
        Console.WriteLine($"{state.Players[state.ActivePlayerNo].NickName} please decide:");

        foreach (var choice in choices)
        {
            Console.WriteLine($"\u001b[1m[{choice.Key}]\u001b[0m {choice.Value.Description()}");
        }

        Console.Write("Your choice: ");
    }

    public static void DrawColorChoices(Dictionary<string, ECardSuit> colors)
    {
        Console.WriteLine($"Choose a new color:");

        foreach (var color in colors)
        {
            Console.WriteLine($"\u001b[1m[{color.Key}]\u001b[0m {color.Value}");
        }
    }

    public static void DrawAiMoves(GameState state, Player aiPlayer, int aiSpeed)
    {
        var previousPlayerMoves = state.PreviousPlayerMoves
            .Where(move => move.Player.Id == aiPlayer.Id)
            .ToList();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(aiPlayer.PlayerType == EPlayerType.AI
            ? $"AI player {aiPlayer.NickName}'s is thinking.."
            : $"Random player {aiPlayer.NickName}'s is thinking..");
        Console.ResetColor();
        Thread.Sleep(aiSpeed);
        if (previousPlayerMoves.Count > 0)
        {
            foreach (var move in previousPlayerMoves)
            {
                Console.WriteLine($"{VisualizeGameCard(move.Card)}- {move.EffectMessage}");
                Thread.Sleep(aiSpeed);
            }

            Console.WriteLine();
        }

        Thread.Sleep(aiSpeed);
    }

    public static void DrawPreviousPlayerMoves(GameState state, Player previousPlayer)
    {
        var previousPlayerMoves = state.PreviousPlayerMoves
            .Where(move => move.Player == previousPlayer)
            .ToList();

        if (previousPlayerMoves.Count > 0)
        {
            Console.WriteLine(previousPlayer.PlayerType == EPlayerType.AI
                ? $"Previous player [AI] {previousPlayer.NickName}'s last moves:"
                : $"Previous player {previousPlayer.NickName}'s last moves:");

            foreach (var move in previousPlayerMoves)
            {
                Console.WriteLine($"{VisualizeGameCard(move.Card)}- {move.EffectMessage}");
            }

            Console.WriteLine();
        }
    }

    public static void DrawWinner(Player winner, GameState state, List<Player> playersInOrder)
    {
        Console.Clear();
        Console.WriteLine($"Congratulations to the winner {winner.NickName}!");
        Console.WriteLine("Thanks for playing UNO!");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(
            $"     _____{winner.NickName}_____\n    |@@@@|     |####|\n    |@@@@|     |####|\n    |@@@@|     |####|\n    \\@@@@|     |####/\n     \\@@@|     |###/\n      `@@|_____|##'\n           (O)\n        .-'''''-.\n      .'  * * *  `.\n     :  *       *  :\n    : ~    {DateTime.Now.Year}   ~ :\n    : ~   WINNER  ~ :\n     :  *       *  :\n      `.  * * *  .'\n        `-.....-'");
        Console.ResetColor();
        Console.WriteLine();
        DrawDesk(state, playersInOrder);
        Console.ReadLine();
    }

    public static string VisualizeGameCard(GameCard? card)
    {
        if (card == null)
        {
            return "     ";
        }

        var valueDescription = FormatValueDescription(card.CardValue.Description(), 4);
        var coloredValue = $"{GetColorCode(card.CardSuit)}{valueDescription}\u001b[0m";

        if (
            card.CardValue == ECardValue.Reverse ||
            card.CardValue == ECardValue.DrawTwo ||
            card.CardValue == ECardValue.WildDrawFour)
        {
            return $"{coloredValue} ";
        }

        return $"{coloredValue}";
    }

    private static string GetColorCode(ECardSuit cardSuit)
    {
        return cardSuit switch
        {
            ECardSuit.Red => "\u001b[41;30m",
            ECardSuit.Blue => "\u001b[44;30m",
            ECardSuit.Green => "\u001b[42;30m",
            ECardSuit.Yellow => "\u001b[43;30m",
            ECardSuit.Wild => "\u001b[47;30m",
            _ => "\u001b[0m"
        };
    }

    private static string FormatValueDescription(string valueDescription, int valueLength)
    {
        valueDescription = valueDescription.Trim();


        if (valueDescription.Length == valueLength)
        {
            return valueDescription;
        }

        var padding = Math.Max(0, (valueLength - valueDescription.Length) / 2);

        var formattedValue = valueDescription.PadLeft(valueDescription.Length + padding).PadRight(valueLength);

        return formattedValue;
    }
}