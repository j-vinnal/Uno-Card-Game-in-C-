namespace Domain;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NickName { get; set; } = default!;
    public EPlayerType PlayerType { get; set; }
    public List<GameCard> PlayerHand { get; set;} = [];
    public bool PlayerHasDrawnCard { get; set; }
    public bool PlayerSkipTurn { get; set; }
    
    public bool WildDraw4Challenge { get; set; }
    
    public bool HasCalledUno { get; set; }

    public bool ChooseColor { get; set; }

}