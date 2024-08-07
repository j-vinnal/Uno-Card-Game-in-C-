using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Games
{
    public class DetailsModel : PageModel
    {
    
        private readonly IGameRepository _gameRepository;
        
        public DetailsModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public Game Game { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = _gameRepository.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            else
            {
                Game = game;
            }
            return Page();
        }
    }
}
