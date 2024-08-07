using Domain;
using Domain.Database;

namespace DAL;

public interface IGameRepository
{
    void Save(Guid id, GameState state);
    void Save(Guid id, String state);
    
    List<(Guid id, DateTime dt)> GetSaveGames();

    List<Game> GetAllGames();

    void Remove(Guid id);

    GameState LoadGame(Guid id);

    Game? FindAsync(Guid? id);
}