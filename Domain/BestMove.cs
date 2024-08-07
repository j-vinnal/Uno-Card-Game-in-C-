namespace Domain;

public class BestMove
{
    public GameCard Card { get; set; } = default!;
    public List<SimulationResult> Results  { get; set; } = [];
    
}