namespace Domain;

public class GameOptions
{
    
    public int HandSize { get; set; } = 7;
    
    public int AiSpeed { get; set; } = 1500;
    public override string ToString() => $"hand size: {HandSize}, AI move speed: {AiSpeed * 0.001} sek";
    
}