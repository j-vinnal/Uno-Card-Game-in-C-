namespace Domain.Database;

public class Game: BaseEntity
{
    public DateTime CreatedAtDt { get; set; } = DateTime.Now;
    public DateTime UpdatedAtDt { get; set; } = DateTime.Now;

    public string State { get; set; } = default!;

    public ICollection<Player>? Players { get; set; }
}