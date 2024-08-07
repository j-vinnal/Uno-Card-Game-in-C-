using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Games
{
    public class IndexModel : PageModel
    {
        private readonly IGameRepository _gameRepository;
        

        public IndexModel(DAL.AppDbContext context, IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public IList<Game> Game { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Game = _gameRepository.GetAllGames();
        }
    }
}
