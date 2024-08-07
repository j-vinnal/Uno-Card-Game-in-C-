using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IGameRepository _gameRepository;
    public int Count { get; set; }
    
    [BindProperty]
    public Game Game { get; set; } = default!;

    public IndexModel(ILogger<IndexModel> logger, IGameRepository gameRepository)
    {
        _logger = logger;
        _gameRepository = gameRepository;
    }
    
    public IList<Game> Games { get;set; } = default!;

    public Task OnGetAsync()
    {
        
        Games = _gameRepository.GetAllGames();
        Count = Games.Count;
        return Task.CompletedTask;
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