using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Games
{
    public class EditModel : PageModel
    {
        private readonly IGameRepository _gameRepository;

        public EditModel(IGameRepository gameRepository)
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
            Game = game;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _gameRepository.Save(Game.Id, Game.State);

            return RedirectToPage("./Index");
        }

        private bool GameExists(Guid id)
        {
            var game = _gameRepository.FindAsync(id);
            return game != null;
        }
    }
}
