using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.PartialViews;

public class Card : PageModel
{
    
    public string GetCardColor(ECardSuit suit)
    {
        switch (suit)
        {
            case ECardSuit.Red:
                return "red";
            case ECardSuit.Blue:
                return "blue";
            case ECardSuit.Green:
                return "green";
            case ECardSuit.Yellow:
                return "yellow";
            default:
                return "";
        }
    }
    
    public string GetCardValueInt(ECardValue value)
    {
        switch (value)
        {
            case ECardValue.Zero:
            case ECardValue.One:
            case ECardValue.Two:
            case ECardValue.Three:
            case ECardValue.Four:
            case ECardValue.Five:
            case ECardValue.Six:
            case ECardValue.Seven:
            case ECardValue.Eight:
            case ECardValue.Nine:
                return ((int)value).ToString();
            default:
                return "";
        }
        
        
    }
}