using Domain;
using Player = Domain.Player;

namespace GameEngine;

public class UnoGameEngine(GameOptions gameOptions)
{
    public GameState State { get; set; } = new GameState();
    private Random Rnd { get; set; } = new Random();
    private int MoveCount { get; set; }

    public void ShuffleAndDealCards()
    {
        for (var cardSuit = 0; cardSuit <= (int)ECardSuit.Green; cardSuit++)
        {
            // 1 x Zero Cards
            AddCardsToDeck((ECardSuit)cardSuit, ECardValue.Zero);

            // 2 x 1-9 cards and special cards
            for (var cardValue = 1; cardValue <= (int)ECardValue.DrawTwo; cardValue++)
            {
                AddCardsToDeck((ECardSuit)cardSuit, (ECardValue)cardValue, 2);
            }

            // 1 x Wild cards
            for (var wildCardValue = (int)ECardValue.Wild;
                 wildCardValue <= (int)ECardValue.WildDrawFour;
                 wildCardValue++)
            {
                AddCardsToDeck(ECardSuit.Wild, (ECardValue)wildCardValue);
            }
        }

        State.DrawPile = ShuffledDeck();
        DealHands();
        MakeAFirstMove();
    }

    private List<GameCard> ShuffledDeck()
    {
        var shuffledDeck = new List<GameCard>();

        while (State.DrawPile.Count > 0)
        {
            var randomPositionInDeck = Rnd.Next(State.DrawPile.Count);
            shuffledDeck.Add(State.DrawPile[randomPositionInDeck]);
            State.DrawPile.RemoveAt(randomPositionInDeck);
        }

        return shuffledDeck;
    }


    private void DealHands()
    {
        foreach (var player in State.Players)
        {
            for (var i = 0; i < gameOptions.HandSize; i++)
            {
                player.PlayerHand.Add(State.DrawPile.Last());
                State.DrawPile.RemoveAt(State.DrawPile.Count - 1);
            }
        }
    }

    public int GetAisPeed()
    {
        return gameOptions.AiSpeed;
    }

    private void MakeAFirstMove()
    {
        while (IsSpecialCard(State.DrawPile[^1]))
        {
            State.DiscardPile.Add(State.DrawPile[^1]);
            State.DrawPile.RemoveAt(State.DrawPile.Count - 1);
        }

        {
            State.DiscardPile.Add(State.DrawPile[^1]);
            State.DrawPile.RemoveAt(State.DrawPile.Count - 1);
        }
    }

    private void AddCardsToDeck(ECardSuit cardSuit, ECardValue cardValue, int cardCount = 1)
    {
        for (var i = 0; i < cardCount; i++)
        {
            State.DrawPile.Add(new GameCard()
            {
                CardValue = cardValue,
                CardSuit = cardSuit,
                CardShortcut = GenerateShortcut(cardSuit, cardValue)
            });
        }
    }


    private string GenerateShortcut(ECardSuit cardSuit, ECardValue cardValue)
    {
        var suitShortcut = cardSuit switch
        {
            ECardSuit.Blue => "B",
            ECardSuit.Green => "G",
            ECardSuit.Red => "R",
            ECardSuit.Yellow => "Y",
            ECardSuit.Wild => "",
            _ => "-"
        };

        var valueShortcut = cardValue switch
        {
            ECardValue.Zero => "0",
            ECardValue.One => "1",
            ECardValue.Two => "2",
            ECardValue.Three => "3",
            ECardValue.Four => "4",
            ECardValue.Five => "5",
            ECardValue.Six => "6",
            ECardValue.Seven => "7",
            ECardValue.Eight => "8",
            ECardValue.Nine => "9",
            ECardValue.Skip => "S",
            ECardValue.Reverse => "R",
            ECardValue.DrawTwo => "D2",
            ECardValue.Wild => "W",
            ECardValue.WildDrawFour => "D4",
            _ => "-"
        };

        var shortcut = suitShortcut + valueShortcut;
        return shortcut;
    }

    public List<GameCard> GetOrderedHand(Player player)
    {
        return player.PlayerHand
            .OrderBy(card => ValidateMove(card, GetTopCard()))
            .ThenBy(card => card.CardSuit)
            .ThenBy(card => card.CardValue).ToList();
    }

    private static bool IsSpecialCard(GameCard card)
    {
        return
            card.CardValue is ECardValue.Wild or ECardValue.WildDrawFour or ECardValue.Skip or ECardValue.Reverse
                or ECardValue.DrawTwo;
    }

    public GameCard GetTopCard()
    {
        return State.DiscardPile[^1];
    }

    public ETurnResult PlayCard(GameCard playedCard)
    {
        var move = new GameMove
        {
            Player = GetActivePlayer(),
            Card = playedCard,
            EffectMessage = GetEffectMessage(playedCard)
        };
        State.PreviousPlayerMoves.Add(move);

        MoveCount++;
        
        DiscardCard(GetActivePlayer(), playedCard);
        switch (playedCard.CardValue)
        {
            case ECardValue.Wild:
                GetActivePlayer().ChooseColor = true;
                return ETurnResult.ActivePlayerTurnContinues;
            case ECardValue.WildDrawFour:
                GetActivePlayer().ChooseColor = true;
                return ETurnResult.ActivePlayerTurnContinues;
            case ECardValue.Skip:
                return NextPlayerSkipTurn();
            case ECardValue.DrawTwo:
                TakeCards(GetNextPlayer(), 2);
                return NextPlayerSkipTurn();
            case ECardValue.Reverse:
                return ToggleDirection();
            default:
                NextPlayerTurn();
                return ETurnResult.NextPlayerTurn;
        }
    }

    private List<GameCard> GetValidMoves()
    {
        return GetActivePlayer().PlayerHand.Where(card => ValidateMove(card, GetTopCard())).ToList();
    }

    private GameCard? ChooseRandomCard()
    {
        var validMoves = GetValidMoves();
        if (validMoves.Count == 0)
        {
            return null;
        }

        var randomIndex = Rnd.Next(validMoves.Count);
        return validMoves[randomIndex];
    }

    private GameCard? ChooseCardByColorAndValue()
    {
        var validMoves = GetValidMoves();
        var playerHand = GetActivePlayer().PlayerHand;

        var preferredValues = new List<ECardValue>
            { ECardValue.Skip, ECardValue.Reverse, ECardValue.DrawTwo, ECardValue.WildDrawFour, ECardValue.Wild };

        var specialCard = validMoves
            .MaxBy(card => preferredValues.IndexOf(card.CardValue));

        var exactMatch = validMoves.FirstOrDefault(card =>
            card.CardSuit == GetTopCard().CardSuit && card.CardValue == GetTopCard().CardValue);

        var maxColorCard = playerHand
            .Where(card => card.CardSuit == GetTopCard().CardSuit)
            .GroupBy(card => card.CardValue).MaxBy(group => group.Count())?
            .FirstOrDefault();

        return specialCard ?? exactMatch ?? maxColorCard ?? ChooseRandomCard();
    }

    private ECardSuit ChooseRandomColor()
    {
        var colors = GetAllColors.Values.ToList();
        var randomIndex = Rnd.Next(colors.Count);
        return colors[randomIndex];
    }

    private ECardSuit ChooseColorByMostCards()
    {
        var playerHand = GetActivePlayer().PlayerHand;

        var preferredValues = new List<ECardValue>
            { ECardValue.Skip, ECardValue.Reverse, ECardValue.DrawTwo };

        var specialCard = playerHand
            .MaxBy(card => preferredValues.IndexOf(card.CardValue));
        if (specialCard != null)
        {
            return specialCard.CardSuit;
        }

        var colorCounts = playerHand
            .Where(card => card.CardSuit is not (ECardSuit.Wild or ECardSuit.Undefined))
            .GroupBy(card => card.CardSuit)
            .ToDictionary(group => group.Key, group => group.Count());

        if (colorCounts.Count != 0)
        {
            var mostCommonColor = colorCounts.OrderByDescending(pair => pair.Value).FirstOrDefault().Key;
            return mostCommonColor;
        }

        return ChooseRandomColor();
    }


    private GameState CreateStateCopy()
    {
        var newState = new GameState
        {
            Id = State.Id,
            ActivePlayerNo = State.ActivePlayerNo,
            IsClockwise = State.IsClockwise,
            Players = State.Players.Select(player =>
            {
                return new Player
                {
                    Id = player.Id,
                    NickName = player.NickName,
                    PlayerType = player.PlayerType,
                    PlayerHand = player.PlayerHand.Select(card =>
                    {
                        var newCard = new GameCard
                        {
                            CardSuit = card.CardSuit,
                            CardValue = card.CardValue,
                            CardShortcut = GenerateShortcut(card.CardSuit, card.CardValue)
                        };
                        return newCard;
                    }).ToList(),
                    PlayerHasDrawnCard = player.PlayerHasDrawnCard,
                    PlayerSkipTurn = player.PlayerSkipTurn,
                    WildDraw4Challenge = player.WildDraw4Challenge,
                    HasCalledUno = player.HasCalledUno
                };
            }).ToList(),
            DrawPile = State.DrawPile.Select(card => new GameCard
            {
                CardSuit = card.CardSuit,
                CardValue = card.CardValue,
                CardShortcut = GenerateShortcut(card.CardSuit, card.CardValue)
            }).ToList(),
            DiscardPile = State.DiscardPile.Select(card => new GameCard
            {
                CardSuit = card.CardSuit,
                CardValue = card.CardValue,
                CardShortcut = GenerateShortcut(card.CardSuit, card.CardValue)
            }).ToList(),
            PreviousPlayerMoves = State.PreviousPlayerMoves.Select(move => new GameMove
            {
                Player = move.Player,
                Card = move.Card,
                EffectMessage = move.EffectMessage
            }).ToList()
        };

        return newState;
    }


    public void AiTurn(bool simulation = false)
    {
        var result = ETurnResult.ActivePlayerTurnContinues;

        while (result == ETurnResult.ActivePlayerTurnContinues && !IsGameOver())
        {
            var availableChoices = GetAvailableChoices();

            if (GetActivePlayer().PlayerHand.Count == 1 && availableChoices.Any(c => c.Value == EPlayerAction.CallUno))
            {
                CallUno();
            }

            if (availableChoices.Any(c => c.Value == EPlayerAction.PlayCard))
            {
                if (simulation)
                {
                    var randomCard = RandomCardPlay();
                    var strategyCard = ChooseCardByColorAndValue()!;
                    var randOrStrategy = Rnd.Next(2);

                    if (randOrStrategy == 0)
                    {
                        if (strategyCard.CardValue is ECardValue.Wild or ECardValue.WildDrawFour)
                        {
                            PlayCard(strategyCard);
                            result = SetColorForTopCard(ChooseColorByMostCards());
                           
                        }
                        else
                        {
                            result = PlayCard(strategyCard);
                        }
                    }
                    else
                    {
                        if (randomCard.CardValue is ECardValue.Wild or ECardValue.WildDrawFour)
                        {
                            
                            
                            
                            PlayCard(randomCard);
                            result = SetColorForTopCard(ChooseRandomColor());
                           
                        }
                        else
                        {
                            result = PlayCard(randomCard);
                        }
                        
                      
                    }
                }
                else
                {
                    var validMoves = GetValidMoves();
                    if (validMoves.Count == 1 && validMoves[0].CardSuit != ECardSuit.Wild)
                    {
                        result = PlayCard(GetValidMoves()[0]);
                    }
                    else
                    {
                        var bestMoves = new List<BestMove>();


                        foreach (var move in validMoves.Distinct())
                        {
                            var gameStateCopy = CreateStateCopy();

                            if (move.CardValue == ECardValue.Wild || move.CardValue == ECardValue.WildDrawFour)
                            {
                                var allColors = GetAllColors;
                                foreach (var color in allColors.Values)
                                {
                                    gameStateCopy = CreateStateCopy();
                                    
                                    PlayCard(move);
                                    SetColorForTopCard(color);

                                    var colorCard = new GameCard
                                    {
                                        CardValue = move.CardValue,
                                        CardSuit = color,
                                        CardShortcut = move.CardShortcut
                                    };

                                    
                                    var simulationResults = SimulateGames(300);
                                    bestMoves.Add(new BestMove()
                                    {
                                        Card = colorCard,
                                        Results = simulationResults,
                                    });

                                    State = gameStateCopy;
                                }
                            }
                            else
                            {
                                PlayCard(move);

                                var simulationResults = SimulateGames(300);
                                bestMoves.Add(new BestMove()
                                {
                                    Card = move,
                                    Results = simulationResults
                                });

                                State = gameStateCopy;
                            }
                        }

                        var activePlay = GetActivePlayer();

                        var orderedMoves = bestMoves.OrderBy(move =>
                        {
                            var activePlayerWins = move.Results
                                .Where(r => r.PlayerId == activePlay.Id)
                                .Sum(r => r.Wins);


                            var cardValueOrder = new Dictionary<ECardValue, int>
                            {
                                { ECardValue.Wild, 0 },
                                { ECardValue.WildDrawFour, 1 },
                                { ECardValue.DrawTwo, 2 },
                                { ECardValue.Skip, 3 },
                                { ECardValue.Reverse, 4 },
                            };

                            var cardOrder = cardValueOrder.GetValueOrDefault(move.Card.CardValue, int.MaxValue);

                            return (activePlayerWins, cardOrder);
                        });

                        var bestMove = orderedMoves.FirstOrDefault();

                        GameCard? card;
                        if (bestMove!.Card.CardValue is ECardValue.Wild or ECardValue.WildDrawFour)
                        {
                            var wildCard = activePlay.PlayerHand.FirstOrDefault(c =>
                                c.CardValue == bestMove.Card.CardValue);

                            PlayCard(wildCard!);
                            result = SetColorForTopCard(bestMove.Card.CardSuit);
                            
                        }
                        else
                        {
                            card = activePlay.PlayerHand.FirstOrDefault(c =>
                                c.CardValue == bestMove.Card.CardValue && c.CardSuit == bestMove.Card.CardSuit);

                            result = PlayCard(card!);
                        }
                    }
                }
            }
            else if (!IsGameOver() && availableChoices.Any(c => c.Value == EPlayerAction.DrawCard) &&
                     availableChoices.Any(c => c.Value != EPlayerAction.PlayCard))
            {
                TakeCards(GetActivePlayer(), 1);
                SetPlayerHasDrawnCard();

                var move = new GameMove
                {
                    Player = GetActivePlayer(),
                    Card = null,
                    EffectMessage = $"{GetActivePlayer().NickName} drew a card"
                };
                State.PreviousPlayerMoves.Add(move);
            }
            else if (GetActivePlayer().WildDraw4Challenge && availableChoices.Any(c =>
                         c.Value is EPlayerAction.SkipTurnWd4 or EPlayerAction.ChallengeWd4))
            {
                var decision = Rnd.Next(2);

                if (decision == 1)
                {
                    var move = new GameMove
                    {
                        Player = GetActivePlayer(),
                        Card = null,
                        EffectMessage = $"{GetActivePlayer().NickName} draw four cards"
                    };
                    State.PreviousPlayerMoves.Add(move);

                    SkipTurnWd4();
                    result = ETurnResult.NextPlayerTurn;
                }
                else
                {
                    var move1 = new GameMove
                    {
                        Player = GetActivePlayer(),
                        Card = null,
                        EffectMessage =
                            $"{GetActivePlayer().NickName} challenged the use of the Wild Draw Four"
                    };
                    State.PreviousPlayerMoves.Add(move1);


                
                    if (ChallengeWd4())
                    {
                        result = ETurnResult.NextPlayerTurn;
                    }
                }
            }
            else if (!IsGameOver() && availableChoices.Any(c => c.Value == EPlayerAction.EndTurn))
            {
                var move = new GameMove
                {
                    Player = GetActivePlayer(),
                    Card = null,
                    EffectMessage = $"{GetActivePlayer().NickName} ended their turn"
                };
                State.PreviousPlayerMoves.Add(move);


                NextPlayerTurn();
                result = ETurnResult.NextPlayerTurn;
            }

            if (IsGameOver())
            {
                break;
            }
        }
    }


    public void SkipTurnWd4()
    {
        TakeCards(GetActivePlayer(), 4);
        GetActivePlayer().WildDraw4Challenge = false;
        NextPlayerTurn();
        
    }


    private GameCard RandomCardPlay()
    {
        var validMoves = GetValidMoves();

        GameCard cardToPlay;
        
        cardToPlay = ChooseRandomCard() ?? validMoves.First();
        

        return cardToPlay;
    }

    private void Simulation()
    {
        MoveCount = 0;

        while (MoveCount <= State.Players.Count * 3 && !IsGameOver())
        {
            AiTurn(true);
        }

        MoveCount = 0;
    }

    private List<SimulationResult> SimulateGames(int numSimulations)
    {
        var results = new List<SimulationResult>();

        for (var i = 0; i < numSimulations; i++)
        {
            var gameStateCopy = CreateStateCopy();
            
            Simulation();

            foreach (var player in State.Players)
            {
                var existingResult = results.FirstOrDefault(result => result.PlayerId == player.Id);
                if (existingResult != null)
                {
                    int cardDifference = player.PlayerHand.Count -
                                         State.Players.Where(p => p.Id != player.Id).Sum(p => p.PlayerHand.Count);
                    existingResult.Wins += cardDifference;
                }
                else
                {
                    int cardDifference = player.PlayerHand.Count -
                                         State.Players.Where(p => p.Id != player.Id).Sum(p => p.PlayerHand.Count);
                    results.Add(new SimulationResult { PlayerId = player.Id, Wins = cardDifference });
                }
            }

            State = gameStateCopy;
        }

        return results;
    }

    public bool CheckUnoCall()
    {
        var activePlayer = GetActivePlayer();
        
        if (activePlayer.PlayerHand.Count == 1 && activePlayer is { HasCalledUno: false, PlayerSkipTurn: false })
        {
            TakeCards(activePlayer, 2);
            return false;
        }

        return true;
    }

    public bool CallUno()
    {
        var move = new GameMove
        {
            Player = GetActivePlayer(),
            Card = null,
            EffectMessage = $"{GetActivePlayer().NickName} called UNO"
        };
        State.PreviousPlayerMoves.Add(move);


        if (GetActivePlayer().PlayerHand.Count != 1)
        {
            TakeCards(GetActivePlayer(), 2);
            return false;
        }

        GetActivePlayer().HasCalledUno = true;
        return true;
    }

    private void DiscardCard(Player player, GameCard card)
    {
        player.PlayerHand.Remove(card);
        State.DiscardPile.Add(card);
    }

    private string GetEffectMessage(GameCard card)
    {
        var nextPlayer = GetNextPlayer();
        return card.CardValue switch
        {
            ECardValue.Wild => "played a Wild card and chose a new color",
            ECardValue.WildDrawFour =>
                $"played a Wild Draw Four card, chose a new color, {nextPlayer.NickName} you have two options for your turn",
            ECardValue.Skip => $"played a Skip card and {nextPlayer.NickName} skipped a turn",
            ECardValue.DrawTwo =>
                $"played a Draw Two card, {nextPlayer.NickName} skipped a turn and draws two cards",
            ECardValue.Reverse => State.Players.Count == 2
                ? "played a Reverse card, the game direction remains the same"
                : $"played a Reverse card, changing game direction to {(State.IsClockwise ? "Counterclockwise" : "Clockwise")}",
            _ => "played a card"
        };
    }

    public void ClearMovesHistory()
    {
        var activePlayer = GetActivePlayer();
        State.PreviousPlayerMoves.RemoveAll(move => move.Player.Id != activePlayer.Id);
    }

    public GameCard? GetSelectedCard(string cardShortcut)
    {
        var activePlayer = GetActivePlayer();
        var card = activePlayer.PlayerHand.FirstOrDefault(card => card.CardShortcut == cardShortcut);

        return card;
    }

    public bool ValidateMove(GameCard playedCard, GameCard topCard)
    {
        var activePlayer = GetActivePlayer();

        if (activePlayer.PlayerSkipTurn || activePlayer.ChooseColor || activePlayer.WildDraw4Challenge)
        {
            return false;
        }
        
        return playedCard.CardSuit == topCard.CardSuit || playedCard.CardValue == topCard.CardValue
                                                       || playedCard.CardValue == ECardValue.Wild ||
                                                       playedCard.CardValue == ECardValue.WildDrawFour;
    }
    
    
    
    public bool HaveCardOfSameColor(Player player, GameCard? card)
    {
        var topCard = card ?? GetTopCard();
        return player.PlayerHand.Any(c => c.CardSuit == topCard.CardSuit);
    }

    private ETurnResult ToggleDirection()
    {
        if (State.Players.Count == 2)
        {
            GetActivePlayer().PlayerHasDrawnCard = false;
            return ETurnResult.ActivePlayerTurnContinues;
        }

        State.IsClockwise = !State.IsClockwise;
        NextPlayerTurn();
        return ETurnResult.NextPlayerTurn;
    }

    public void NextPlayerTurn()
    {
        GetActivePlayer().PlayerHasDrawnCard = false;
        GetActivePlayer().PlayerSkipTurn = false;
        GetActivePlayer().HasCalledUno = false;
        
        ClearMovesHistory();
        
        var playerCount = State.Players.Count;

        if (State.IsClockwise)
        {
            State.ActivePlayerNo++;
            if (State.ActivePlayerNo >= playerCount)
            {
                State.ActivePlayerNo = 0;
            }
        }
        else
        {
            State.ActivePlayerNo--;
            if (State.ActivePlayerNo < 0)
            {
                State.ActivePlayerNo = playerCount - 1;
            }
        }
    }
    
    public List<Player> GetPlayersInOrder(Guid? firstPlayerId = null, Boolean gameOrder = true)
    {
        var players = State.Players.ToList();

        if (gameOrder)
        {
            if (State.IsClockwise)
            {
                players.Sort((player1, player2) =>
                    (State.Players.IndexOf(player1) - State.ActivePlayerNo + State.Players.Count) % State.Players.Count
                    - (State.Players.IndexOf(player2) - State.ActivePlayerNo + State.Players.Count) %
                    State.Players.Count);
            }
            else
            {
                players.Sort((player1, player2) =>
                    (State.Players.IndexOf(player2) - State.ActivePlayerNo + State.Players.Count) % State.Players.Count
                    - (State.Players.IndexOf(player1) - State.ActivePlayerNo + State.Players.Count) %
                    State.Players.Count);
            }
        }
        else
        {
            players.Sort((player1, player2) =>
                (State.Players.IndexOf(player1) - State.ActivePlayerNo + State.Players.Count) % State.Players.Count
                - (State.Players.IndexOf(player2) - State.ActivePlayerNo + State.Players.Count) %
                State.Players.Count);
        }

        // Rotate the list so that the player on turn - or active player is always the first
        int? firstPlayerIndex = null;
        
        if (firstPlayerId != null)
        {
            var firstPlayer = players.FirstOrDefault(p => p?.Id == firstPlayerId);
            firstPlayerIndex = firstPlayer != null ? players.IndexOf(firstPlayer) : null;
        }
        
        var rotationIndex = firstPlayerIndex ?? players.IndexOf(State.Players[State.ActivePlayerNo]);
        players = players.Skip(rotationIndex).Concat(players.Take(rotationIndex)).ToList();

        return players;
    }


    private void WildDraw4Played()
    {
        GetNextPlayer().WildDraw4Challenge = true;
        NextPlayerTurn();
    }

    public bool ChallengeWd4()
    {
        GetActivePlayer().WildDraw4Challenge = false;
        var previousPlayer = GetPreviousPlayer();
        if (HaveCardOfSameColor(previousPlayer, State.DiscardPile[^2]))
        {
            var move1 = new GameMove
            {
                Player = GetActivePlayer(),
                Card = null,
                EffectMessage =
                    $"Challenge is successful, {GetPreviousPlayer().NickName} must draw four cards"
            };
            State.PreviousPlayerMoves.Add(move1);

            TakeCards(previousPlayer, 4);
            State.PreviousPlayerMoves.RemoveAll(move => move.Player != GetActivePlayer());
            return false;
        }

        var move = new GameMove
        {
            Player = GetActivePlayer(),
            Card = null,
            EffectMessage =
                $"Challenge fails, {GetActivePlayer().NickName} draw six cards and miss turn"
        };
        State.PreviousPlayerMoves.Add(move);

        TakeCards(GetActivePlayer(), 6);
        NextPlayerTurn();
        return true;
    }

    private ETurnResult NextPlayerSkipTurn()
    {
        if (State.Players.Count == 2)
        {
            GetActivePlayer().PlayerHasDrawnCard = false;
            return ETurnResult.ActivePlayerTurnContinues;
        }

        NextPlayerTurn();
        GetActivePlayer().PlayerSkipTurn = true;
        return ETurnResult.NextPlayerTurn;
    }

    public Player GetActivePlayer()
    {
        return State.Players[State.ActivePlayerNo];
    }

    public Player GetPreviousPlayer()
    {
        return State.IsClockwise ? GetCounterClockwisePlayer() : GetClockwisePlayer();
    }

    public Player GetNextPlayer()
    {
        return State.IsClockwise ? GetClockwisePlayer() : GetCounterClockwisePlayer();
    }

    private Player GetClockwisePlayer()
    {
        var nextPlayerNo = State.ActivePlayerNo + 1;

        if (nextPlayerNo >= State.Players.Count)
        {
            nextPlayerNo = 0;
        }

        return State.Players[nextPlayerNo];
    }

    private Player GetCounterClockwisePlayer()
    {
        var previousPlayerNo = State.ActivePlayerNo - 1 < 0 ? State.Players.Count - 1 : State.ActivePlayerNo - 1;
        return State.Players[previousPlayerNo];
    }

    
    public Dictionary<string, EPlayerAction> GetAvailableChoices(Guid? playerId = null)
    {
        
        var currentPlayerId = playerId ?? GetActivePlayer().Id;
        
        var activePlayer = GetActivePlayer();
        var choices = new Dictionary<string, EPlayerAction>();
        
   

        if (currentPlayerId != activePlayer.Id)
        {
            return choices;
        }
        
        var player = State.Players.FirstOrDefault(p => p.Id == currentPlayerId) ?? null;
        
        
        if (player != null)
        {

            if (player.PlayerSkipTurn)
            {
                choices.Add("E", EPlayerAction.EndTurn);
                choices.Add("SG", EPlayerAction.SaveGame);
                return choices;
            }

            if (player.ChooseColor)
            {
                choices.Add("SG", EPlayerAction.SaveGame);
                return choices;
            }
            
            if (player.WildDraw4Challenge)
            {
                choices.Add("S", EPlayerAction.SkipTurnWd4);

                choices.Add("CW", EPlayerAction.ChallengeWd4);
                choices.Add("SG", EPlayerAction.SaveGame);
                return choices;
            }
            
            if (CanMakeMove(player))
                choices.Add("P", EPlayerAction.PlayCard);

            if (player.PlayerHasDrawnCard && !CanMakeMove())
                choices.Add("E", EPlayerAction.EndTurn);

            if (!player.PlayerHasDrawnCard)
                choices.Add("D", EPlayerAction.DrawCard);

            choices.Add("C", EPlayerAction.CallUno);

            choices.Add("SG", EPlayerAction.SaveGame);
        }

        return choices;
    }


    

    private bool CanMakeMove(Player? player = null)
    {
        var currentPlayer = player ?? GetActivePlayer();
        
        return currentPlayer.PlayerHand.Any(card => ValidateMove(card, GetTopCard()));
    }

    public EPlayerAction GetSelectedChoice(string choiceKey)
    {
        var choices = GetAvailableChoices();
        return choices.GetValueOrDefault(choiceKey, EPlayerAction.Undefined);
    }

    public readonly Dictionary<string, ECardSuit> GetAllColors = new Dictionary<string, ECardSuit>
    {
        { "R", ECardSuit.Red },
        { "B", ECardSuit.Blue },
        { "G", ECardSuit.Green },
        { "Y", ECardSuit.Yellow },
    };

    public ECardSuit GetSelectedColor(string selectedColorKey)
    {
        var colorKey = selectedColorKey.ToUpper().Trim();
        return GetAllColors.GetValueOrDefault(colorKey, ECardSuit.Undefined);
    }

    
    
    public ETurnResult SetColorForTopCard(ECardSuit color)
    {
        GetActivePlayer().ChooseColor = false;
        var topCard = GetTopCard();

        if (topCard.CardSuit == ECardSuit.Wild)
        {

            topCard.CardSuit = color;
            
            if (topCard.CardValue == ECardValue.WildDrawFour)
            {
                WildDraw4Played();
                return ETurnResult.NextPlayerTurn;
            }
        }

        return ETurnResult.ActivePlayerTurnContinues;

    }
    
    
    public bool IsGameOver()
    {
        return State.Players.Any(player => player.PlayerHand.Count == 0);
    }

    public Player? GetWinner()
    {
        return State.Players.FirstOrDefault(player => player.PlayerHand.Count == 0);
    }

    public void SetPlayerHasDrawnCard()
    {
        GetActivePlayer().PlayerHasDrawnCard = true;
        GetActivePlayer().HasCalledUno = false;
    }

    public void TakeCards(Player player, int cardCount)
    {
        if (State.DrawPile.Count < cardCount)
        {
            NewDrawPile();
        }

        for (var i = 0; i < cardCount; i++)
        {
            var drawnCard = State.DrawPile[^1];
            player.PlayerHand.Add(drawnCard);
            State.DrawPile.RemoveAt(State.DrawPile.Count - 1);
        }
    }

    private void NewDrawPile()
    {
        var topDiscard = GetTopCard();
        State.DiscardPile.Remove(topDiscard);

        ResetWildCardSuit();
        
        State.DrawPile = State.DiscardPile;
        State.DrawPile = ShuffledDeck();
        State.DiscardPile.Clear();
        State.DiscardPile.Add(topDiscard);
    }

    private void ResetWildCardSuit()
    {
        foreach (var card in State.DiscardPile.Where(card => card.CardValue is ECardValue.Wild or ECardValue.WildDrawFour))
        {
            card.CardSuit = ECardSuit.Wild;
        }
    }
}