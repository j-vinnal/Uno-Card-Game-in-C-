namespace Domain;

public class GameMove
{
    public Player Player { get; set; } = default!;
    public GameCard? Card { get; set; }
    public string EffectMessage { get; set; } = default!;
}