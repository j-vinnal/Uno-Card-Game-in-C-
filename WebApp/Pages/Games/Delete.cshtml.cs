using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Games
{
    public class DeleteModel : PageModel
    {
        private readonly IGameRepository _gameRepository;

        public DeleteModel(AppDbContext context, IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = _gameRepository.FindAsync(id);
            if (game != null)
            {
                Game = game;
                
                _gameRepository.Remove(Game.Id);
            }

            return RedirectToPage("./Index");
        }
    }
}
