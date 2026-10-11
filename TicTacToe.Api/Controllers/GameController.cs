using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/games")]
public class GameController : ControllerBase
{
    private readonly GameSessionStore _store;

    public GameController(GameSessionStore store)
    {
        _store = store;
    }

    [HttpPost]
    public IActionResult CreateGame([FromBody] CreateGameRequest request)
    {
        var mode = request.Mode switch
        {
            "two-player" => GameMode.TwoPlayer,
            "computer" => GameMode.Computer,
            _ => (GameMode?)null,
        };
        if (mode is null) return BadRequest(new { message = "mode must be two-player or computer." });

        var game = _store.Create(mode.Value);
        return Created($"/api/games/{game.Id}", _store.ToDto(game));
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetGame(Guid id)
    {
        return _store.TryGet(id, out var game)
            ? Ok(_store.ToDto(game))
            : NotFound(new { message = "Game session was not found." });
    }

    [HttpPost("{id:guid}/moves")]
    public IActionResult PlayMove(Guid id, [FromBody] MoveRequest request)
    {
        if (!_store.TryGet(id, out var game)) return NotFound(new { message = "Game session was not found." });
        lock (game.SyncRoot)
        {
            var error = _store.PlayMove(game, request.Position);
            return error is null ? Ok(_store.ToDto(game)) : BadRequest(new { message = error });
        }
    }

    [HttpPost("{id:guid}/undo")]
    public IActionResult UndoMove(Guid id)
    {
        if (!_store.TryGet(id, out var game)) return NotFound(new { message = "Game session was not found." });
        lock (game.SyncRoot)
        {
            _store.Undo(game);
            return Ok(_store.ToDto(game));
        }
    }

    [HttpPost("{id:guid}/reset")]
    public IActionResult ResetGame(Guid id)
    {
        if (!_store.TryGet(id, out var game)) return NotFound(new { message = "Game session was not found." });
        lock (game.SyncRoot)
        {
            _store.ResetGame(game);
            return Ok(_store.ToDto(game));
        }
    }
}
