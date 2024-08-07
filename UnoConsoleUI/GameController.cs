using DAL;
using Domain;
using GameEngine;

namespace UnoConsoleUI;

public class GameController
{
    private readonly UnoGameEngine _engine;
    private readonly IGameRepository _repository;
    private string _continueAnswer = "";

    public GameController(UnoGameEngine engine, IGameRepository repository)
    {
        _engine = engine;
        _repository = repository;
    }


    public void Run()
    {
        //only show when next player move
        Console.Clear();
        while (!_engine.IsGameOver() && _continueAnswer != "N")
        {
            Console.Clear();
           // _engine.ClearMovesHistory();
            ConsoleVisualization.DrawPlayerInfo(_engine.State);

            if (_engine.GetActivePlayer().PlayerType is EPlayerType.AI or EPlayerType.Random)
            {
                Console.WriteLine(_engine.GetActivePlayer().PlayerType == EPlayerType.AI
                    ? $"AI player {_engine.GetActivePlayer().NickName} turn..."
                    : $"Random player {_engine.GetActivePlayer().NickName} turn...");
                Thread.Sleep(_engine.GetAisPeed());
            }
            else
            {
                Console.WriteLine(
                    "Your turn, make sure you are alone looking at the screen! Press enter to continue...");
                Console.ReadLine();
            }

            DrawGameInfo();
        }

        if (_engine.IsGameOver())
        {
            ConsoleVisualization.DrawWinner(_engine.GetWinner()!, _engine.State, _engine.GetPlayersInOrder());
        }
    }

    private void DrawGameInfo()
    {
        var activePlayer = _engine.GetActivePlayer();
        while (true)
        {
            Console.Clear();
            if (activePlayer.PlayerSkipTurn)
            {
                ConsoleVisualization.DrawPlayerSkipTurn(activePlayer, _engine.GetNextPlayer(),
                    _engine.GetPreviousPlayer(), _engine.State, _engine.ValidateMove, _engine.GetTopCard(), _engine.GetAisPeed());
                _engine.NextPlayerTurn();
                break;
            }

            ConsoleVisualization.DrawPreviousPlayerMoves(_engine.State, _engine.GetPreviousPlayer());
            ConsoleVisualization.DrawPlayerInfo(_engine.State);
            ConsoleVisualization.DrawDesk(_engine.State, _engine.GetPlayersInOrder());
            ConsoleVisualization.DrawPlayerHand(activePlayer, _engine.ValidateMove, _engine.GetTopCard(),
                _engine.HaveCardOfSameColor(activePlayer, _engine.GetTopCard()));
            ConsoleVisualization.DrawChoices(_engine.State, _engine.GetAvailableChoices());

            if (_engine.GetActivePlayer().PlayerType == EPlayerType.AI)
            {
                _engine.AiTurn();
                ConsoleVisualization.DrawAiMoves(_engine.State, _engine.GetPreviousPlayer(), _engine.GetAisPeed());
                break;
            }

            if (_engine.GetActivePlayer().PlayerType == EPlayerType.Random)
            {
                _engine.AiTurn(true);
                ConsoleVisualization.DrawAiMoves(_engine.State, _engine.GetPreviousPlayer(), _engine.GetAisPeed());
                break;
            }

            var choiceKey = Console.ReadLine()!.ToUpper().Trim();
            if (!string.IsNullOrWhiteSpace(choiceKey))
            {
                if (ProcessPlayerChoice(choiceKey) == ETurnResult.NextPlayerTurn)
                {
                    break;
                }
            }
        }
    }

    private ETurnResult ProcessPlayerChoice(string choiceKey)
    {
        var selectedChoice = _engine.GetSelectedChoice(choiceKey);
        var activePlayer = _engine.GetActivePlayer();

        switch (selectedChoice)
        {
            case EPlayerAction.PlayCard:
                var checkUnoCall = _engine.CheckUnoCall();
                var playCard = ProcessPlayCardChoice();
                if (!checkUnoCall)
                {
                    ConsoleVisualization.DrawForgotToSayUno();
                }

                return playCard;

            case EPlayerAction.EndTurn:
                if (!_engine.CheckUnoCall())
                {
                    ConsoleVisualization.DrawForgotToSayUno();
                    return ETurnResult.ActivePlayerTurnContinues;
                }
                
                _engine.NextPlayerTurn();
                var move2 = new GameMove
                {
                    Player = activePlayer,
                    Card = null,
                    EffectMessage = $"{activePlayer.NickName} ended their turn"
                };
                _engine.State.PreviousPlayerMoves.Add(move2);
                return ETurnResult.NextPlayerTurn;

            case EPlayerAction.DrawCard:
                if (!_engine.CheckUnoCall())
                {
                    ConsoleVisualization.DrawForgotToSayUno();
                }

                _engine.TakeCards(_engine.GetActivePlayer(), 1);
                _engine.SetPlayerHasDrawnCard();
                
                var move = new GameMove
                {
                    Player = _engine.GetActivePlayer(),
                    Card = null,
                    EffectMessage = $"{_engine.GetActivePlayer().NickName} drew a card"
                };
                _engine.State.PreviousPlayerMoves.Add(move);
                
                return ETurnResult.ActivePlayerTurnContinues;

            case EPlayerAction.SaveGame:
                _repository.Save(_engine.State.Id, _engine.State);
                Console.Write("State saved. Continue (Y/N)[Y]?");
                _continueAnswer = Console.ReadLine()!.Trim().ToUpper();
                return _continueAnswer == "N" ? ETurnResult.NextPlayerTurn : ETurnResult.ActivePlayerTurnContinues;

            case EPlayerAction.CallUno:
                Console.WriteLine(_engine.CallUno()
                    ? "Uno called at the right time!"
                    : "Uno called at the wrong time, 2 penalty cards!");
                Console.ReadLine();
                return ETurnResult.ActivePlayerTurnContinues;
            case EPlayerAction.SkipTurnWd4:
                var moveWd4 = new GameMove
                {
                    Player =activePlayer,
                    Card = null,
                    EffectMessage = $"{activePlayer.NickName} draw four cards"
                };
                _engine.State.PreviousPlayerMoves.Add(moveWd4);
                
                _engine.SkipTurnWd4();
                return ETurnResult.NextPlayerTurn;
            case EPlayerAction.ChallengeWd4:
                var moveCwd4 = new GameMove
                {
                    Player = activePlayer,
                    Card = null,
                    EffectMessage =
                        $"{activePlayer.NickName} challenged the use of the Wild Draw Four"
                };
                _engine.State.PreviousPlayerMoves.Add(moveCwd4);
                Console.WriteLine(
                    $"Top card was: {ConsoleVisualization.VisualizeGameCard(_engine.State.DiscardPile[^2])}");
                ConsoleVisualization.DrawPlayerHand(_engine.GetPreviousPlayer(), _engine.ValidateMove,
                    _engine.State.DiscardPile[^2]);
               
                if (_engine.ChallengeWd4())
                {
                    Console.WriteLine("Challenge fails, you must draw six cards and miss your turn");
                    Console.ReadLine();
                    return ETurnResult.NextPlayerTurn;
                }

                Console.WriteLine(
                    $"Challenge is successful, {_engine.GetPreviousPlayer().NickName} must draw four cards");
                Console.WriteLine($"{_engine.GetActivePlayer().NickName}'s turn continues..");
                Console.ReadLine();
                return ETurnResult.ActivePlayerTurnContinues;
            case EPlayerAction.Undefined:
                Console.WriteLine($"Invalid choice {choiceKey}. Please try again.");
                Console.ReadLine();
                return ETurnResult.ActivePlayerTurnContinues;

            default:
                return ETurnResult.ActivePlayerTurnContinues;
        }
    }

    private ETurnResult ProcessPlayCardChoice()
    {
        Console.Write("Select a card to play: ");
        var cardShortcut = Console.ReadLine()?.ToUpper().Trim();
        if (string.IsNullOrWhiteSpace(cardShortcut)) return ETurnResult.ActivePlayerTurnContinues;

        var playedCard = _engine.GetSelectedCard(cardShortcut);
        if (playedCard != null && _engine.ValidateMove(playedCard, _engine.GetTopCard()))
        {
            return playedCard.CardValue switch
            {
                ECardValue.Wild => ProcessWildCard(playedCard),
                ECardValue.WildDrawFour => ProcessWildCard(playedCard),
                ECardValue.Skip => _engine.PlayCard(playedCard),
                ECardValue.Reverse => _engine.PlayCard(playedCard),
                ECardValue.DrawTwo => _engine.PlayCard(playedCard),
                _ => _engine.PlayCard(playedCard)
            };
        }

        Console.WriteLine($"Invalid card {cardShortcut}. Please try again.");
        Console.ReadLine();
        return ETurnResult.ActivePlayerTurnContinues;
    }

    private ETurnResult ProcessWildCard(GameCard playedCard)
    {
        _engine.PlayCard(playedCard);
        
        ConsoleVisualization.DrawColorChoices(_engine.GetAllColors);
        while (true)
        {
            Console.Write("Your choice: ");
            var colorKey = Console.ReadLine()?.ToUpper().Trim();
            if (!string.IsNullOrWhiteSpace(colorKey))
            {
                var selectedColor = _engine.GetSelectedColor(colorKey);

                if (selectedColor != ECardSuit.Undefined)
                {

                   return _engine.SetColorForTopCard(selectedColor);
                 
                }
            }

            Console.WriteLine($"Wrong color key {colorKey}");
        }
    }
}