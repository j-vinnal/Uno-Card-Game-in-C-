using DAL;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Play;

public class Index : PageModel
{
   
    private readonly IGameRepository _gameRepository;
    private GameOptions _gameOptions = new GameOptions();


    public Player CurrentPlayer { get; set; } = default!;

    [BindProperty(SupportsGet = true)] public ECardSuit SelectedNewColor { get; set; }
    public IList<Player> PlayersInOrder { get; set; } = new List<Player>();

    public UnoGameEngine Engine { get; set; } = default!;

    public Index(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    [BindProperty(SupportsGet = true)] public Guid GameId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid PlayerId { get; set; }

    [BindProperty(SupportsGet = true)] public string CardShortcut { get; set; } = default!;


    public async Task<IActionResult> OnPostPlayCardAsync()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id &&
            Engine.GetAvailableChoices().Any(c => c.Value == EPlayerAction.PlayCard))
        {
            var playedCard = Engine.GetSelectedCard(CardShortcut);

            if (Engine.ValidateMove(playedCard, Engine.GetTopCard()))
            {
                Engine.CheckUnoCall();

                Engine.PlayCard(playedCard);

                _gameRepository.Save(Engine.State.Id, Engine.State);
            }
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }


    public async Task<IActionResult> OnPostSetNewColorAsync()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id && Engine.GetActivePlayer().ChooseColor)
        {
            Engine.SetColorForTopCard(SelectedNewColor);
            _gameRepository.Save(Engine.State.Id, Engine.State);
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }

    public async Task<IActionResult> OnPostSkipTurnWd4Async()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id && Engine.GetActivePlayer().WildDraw4Challenge)
        {
            Engine.SkipTurnWd4();
            _gameRepository.Save(Engine.State.Id, Engine.State);
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }

    public async Task<IActionResult> OnPostChallengeWd4Async()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id && Engine.GetActivePlayer().WildDraw4Challenge)
        {
            Engine.ChallengeWd4();
            _gameRepository.Save(Engine.State.Id, Engine.State);
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }


    public async Task<IActionResult> OnPostDrawCardAsync()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id &&
            Engine.GetAvailableChoices().Any(c => c.Value == EPlayerAction.DrawCard))
        {
            Engine.CheckUnoCall();

            Engine.TakeCards(Engine.GetActivePlayer(), 1);
            Engine.SetPlayerHasDrawnCard();

            _gameRepository.Save(Engine.State.Id, Engine.State);
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }

    public async Task<IActionResult> OnPostCallUnoAsync()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id)
        {
            Engine.CallUno();

            _gameRepository.Save(Engine.State.Id, Engine.State);
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }

    public async Task<IActionResult> OnPostEndTurnAsync()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (PlayerId == Engine.GetActivePlayer().Id &&
            Engine.GetAvailableChoices().Any(c => c.Value == EPlayerAction.EndTurn))
        {
            Engine.CheckUnoCall();


            Engine.NextPlayerTurn();

            _gameRepository.Save(Engine.State.Id, Engine.State);
        }

        return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
    }

    public async Task<IActionResult> OnPostAITurnAsync()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };


        if (Engine.GetActivePlayer().PlayerType == EPlayerType.AI)
        {
            Engine.AiTurn();
            _gameRepository.Save(Engine.State.Id, Engine.State);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (Engine.GetActivePlayer().PlayerType == EPlayerType.Random)
        {
            Engine.AiTurn(true);
            _gameRepository.Save(Engine.State.Id, Engine.State);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

       return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
       
    }

    public async Task<IActionResult> OnGet()
    {
        var gameState = _gameRepository.LoadGame(GameId);

        Engine = new UnoGameEngine(_gameOptions)
        {
            State = gameState
        };

        CurrentPlayer = Engine.State.Players.FirstOrDefault(p => p.Id == PlayerId)!;

        PlayersInOrder = Engine.GetPlayersInOrder(PlayerId, false).FindAll(p => p.Id != PlayerId);


        if (Engine.IsGameOver())
        {
            var winner = Engine.GetWinner();
            return RedirectToPage("/Play/GameOver", new { winnerNickName = winner!.NickName });
        }


        return Page();
    }
}