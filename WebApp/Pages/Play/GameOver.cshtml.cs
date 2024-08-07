using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Play;

public class GameOver : PageModel
{

    [BindProperty(SupportsGet = true)] 
    public String WinnerNickName { get; set; } = default!;
    
    public void OnGet()
    {
        
        
    }
}