using DAL;
using Domain;
using Domain.Database;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Player = Domain.Player;

namespace WebApp.Pages.Games
{
    public class CreateModel : PageModel
    {
       
        private readonly IGameRepository _gameRepository;

        public CreateModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Game Game { get; set; } = default!;

        [BindProperty]
        public GameOptions GameOptions { get; set; } = new GameOptions();


        [BindProperty] 
        public List<Player> Players { get; set; } = new List<Player>();
        
        
        [BindProperty] 
        public Player Player { get; set; } = new Player();

        
        
        public async Task<IActionResult> OnPostAddPlayerAsync()
        {

            if (Players.Count <= 4)
            {
                var newPlayer = new Player
                {
                    NickName = Player.NickName,
                    PlayerType = Player.PlayerType
                };

                Players.Add(newPlayer);

            }

            return Page();
        }


        public async Task<IActionResult> OnPostStartGameAsync()
        {

            if (Players.Count < 2 || Players.Count > 4)
            {
                return Page();
            }
            var gameEngine = new UnoGameEngine(GameOptions)
            {
                State =
                {
                   Players = Players
                }
            };
            
            gameEngine.ShuffleAndDealCards();

            _gameRepository.Save(gameEngine.State.Id, gameEngine.State);
                

            var playerId = gameEngine.State.Players[0].Id;

            return RedirectToPage("/Play/Index", new { gameId = gameEngine.State.Id, playerId = playerId });
            
        }
    }
}
