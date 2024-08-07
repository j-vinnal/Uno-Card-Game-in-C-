using System.Runtime.Serialization;
using System.Text.Json;
using Domain;
using Domain.Database;
using Helpers;

namespace DAL;

public class GameRepositoryFileSystem: IGameRepository
{
    private const string SaveLocation = "C:/Projects/Uni/icd0008-23f/Uno/SavedGames";
    public void Save(Guid id, GameState state)
    {
        var content = JsonSerializer.Serialize(state, JsonHelpers.JsonSerializerOptions);

        var fileName = Path.ChangeExtension(id.ToString(), ".json");

        if (!Path.Exists(SaveLocation))
        {
            Directory.CreateDirectory(SaveLocation);
        }

        File.WriteAllText(Path.Combine(SaveLocation, fileName), content);
    }

    public void Save(Guid id, string state)
    {
        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        var filePath = Path.Combine(SaveLocation, fileName);

        if (!Directory.Exists(SaveLocation))
        {
            Directory.CreateDirectory(SaveLocation);
        }

        File.WriteAllText(filePath, state);
    }

    public List<(Guid id, DateTime dt)> GetSaveGames()
    {
        if (!Directory.Exists(SaveLocation))
        {
            Console.WriteLine($"Error: Directory {SaveLocation} not found.");
            return new List<(Guid id, DateTime dt)>();
        }

        var data = Directory.EnumerateFiles(SaveLocation);
        var res = data
            .Select(
                path => (
                    Guid.Parse(Path.GetFileNameWithoutExtension(path)),
                    File.GetLastWriteTime(path)
                )
            )
            .OrderByDescending<(Guid id, DateTime dt), DateTime>(item => item.dt)
            .ToList();

        return res;
    }

    public List<Game> GetAllGames()
    {
        if (!Directory.Exists(SaveLocation))
        {
            Console.WriteLine($"Error: Directory {SaveLocation} not found.");
            return new List<Game>();
        }

        var data = Directory.EnumerateFiles(SaveLocation);
        var games = new List<Game>();

        foreach (var path in data)
        {
            var id = Guid.Parse(Path.GetFileNameWithoutExtension(path));
            
            var gameState = LoadGame(id);
            var gameStateStr =  File.ReadAllText(path);
            
            var game = new Game
            {
                Id = gameState.Id,
                CreatedAtDt = File.GetCreationTimeUtc(path),
                UpdatedAtDt = File.GetLastWriteTimeUtc(path),
                State = gameStateStr,
                Players =  gameState.Players.Select(p => new Domain.Database.Player()
                {
                    Id = p.Id,
                    NickName = p.NickName,
                    PlayerType = p.PlayerType
                }).ToList()
            };

            games.Add(game);
        }

        return games.OrderByDescending(g => g.UpdatedAtDt).ToList();
    }

    public void Remove(Guid id)
    {
        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        var filePath = Path.Combine(SaveLocation, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }


    public GameState LoadGame(Guid id)
    {
        var fileName = Path.ChangeExtension(id.ToString(), ".json");

        var jsonStr = File.ReadAllText(Path.Combine(SaveLocation, fileName));
        var res = JsonSerializer.Deserialize<GameState>(jsonStr, JsonHelpers.JsonSerializerOptions);
        if (res == null) throw new SerializationException($"Cannot deserialize {jsonStr}");

        return res;
    }

    public Game? FindAsync(Guid? id)
    {
        if (id == null)
        {
            return null; 
        }

        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        var filePath = Path.Combine(SaveLocation, fileName!);

        if (File.Exists(filePath))
        {
            var gameState = LoadGame(id.Value);
            var gameStateStr = File.ReadAllText(filePath);

            return new Game
            {
                Id = gameState.Id,
                CreatedAtDt = File.GetCreationTimeUtc(filePath),
                UpdatedAtDt = File.GetLastWriteTimeUtc(filePath),
                State = gameStateStr,
                Players = gameState.Players.Select(p => new Domain.Database.Player()
                {
                    Id = p.Id,
                    NickName = p.NickName,
                    PlayerType = p.PlayerType
                }).ToList()
            };
        }

        return null; 
    }
    
}