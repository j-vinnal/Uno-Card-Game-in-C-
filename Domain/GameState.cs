namespace Domain;

public class GameState
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public List<Player> Players { get; set; } = [];
    public List<GameCard> DrawPile { get; set; } = [];
    public List<GameCard> DiscardPile { get; set; } = [];
    public int ActivePlayerNo { get; set; }
    public List<GameMove> PreviousPlayerMoves { get; set; } = [];
    public bool IsClockwise { get; set; } = true;
    
}