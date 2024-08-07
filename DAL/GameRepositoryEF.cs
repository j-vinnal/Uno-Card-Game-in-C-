using System.Text.Json;
using Domain;
using Domain.Database;
using Helpers;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class GameRepositoryEF : IGameRepository
{
    private readonly AppDbContext _ctx;

    public GameRepositoryEF(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public void Save(Guid id, GameState state)
    {
        //is it already in db?
        var game = _ctx.Games.FirstOrDefault(g => g.Id == state.Id);
        if (game == null)
        {
            game = new Game()
            {
                Id = state.Id,
                State = JsonSerializer.Serialize(state, JsonHelpers.JsonSerializerOptions),
                Players = state.Players.Select(p => new Domain.Database.Player()
                {
                    Id = p.Id,
                    NickName = p.NickName,
                    PlayerType = p.PlayerType
                }).ToList()
            };

            _ctx.Games.Add(game);
        }
        else
        {
            game.UpdatedAtDt = DateTime.Now;
            game.State = JsonSerializer.Serialize(state, JsonHelpers.JsonSerializerOptions);
        }

        _ctx.SaveChanges();
    }

    public void Save(Guid id, string state)
    {
        var game = _ctx.Games.FirstOrDefault(g => g.Id == id);

        if (game == null)
        {
            game = new Game()
            {
                Id = id,
                State = state,
                UpdatedAtDt = DateTime.Now,
                CreatedAtDt = DateTime.Now
             
            };

            _ctx.Games.Add(game);
        }
        else
        {
            game.State = state;
            game.UpdatedAtDt = DateTime.Now;
          
        }

        _ctx.SaveChanges();
    }

    public List<(Guid id, DateTime dt)> GetSaveGames()
    {
        return _ctx.Games
            .OrderByDescending(g => g.UpdatedAtDt)
            .ToList()
            .Select(g => (g.Id, g.UpdatedAtDt))
            .ToList();
    }

    public List<Game> GetAllGames()
    {
        return
            _ctx.Games
                .Include(g => g.Players)
                .OrderByDescending(g => g.UpdatedAtDt)
                .ToList();
    }

    public void Remove(Guid id)
    {
        var game = _ctx.Games.FirstOrDefault(g => g.Id == id);

        if (game != null)
        {
            _ctx.Games.Remove(game);
            _ctx.SaveChanges();
        }
    }
    
    public GameState LoadGame(Guid id)
    {
        var game = _ctx.Games.First(g => g.Id == id);
        return JsonSerializer.Deserialize<GameState>(game.State, JsonHelpers.JsonSerializerOptions)!;
    }

    public Game? FindAsync(Guid? id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return _ctx.Games.FirstOrDefault(g => g.Id == id.Value);
    }
}