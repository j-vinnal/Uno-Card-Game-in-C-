namespace Domain;

public class SimulationOptions
{
    public int SimulationCount { get; set; } = 10;
    
    public override string ToString() => $"{SimulationCount} games";
}