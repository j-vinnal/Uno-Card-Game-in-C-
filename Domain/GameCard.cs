namespace Domain;

public class GameCard
{
    public ECardSuit CardSuit { get; set; }
    public ECardValue CardValue { get; set; }
    public string CardShortcut { get; set; } = default!;
    
}