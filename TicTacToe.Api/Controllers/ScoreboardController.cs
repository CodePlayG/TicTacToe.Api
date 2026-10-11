using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly GameSessionStore _store;

    public ScoreboardController(GameSessionStore store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult GetScoreboard()
    {
        return Ok(_store.ScoreboardSnapshot());
    }

    [HttpPost("reset")]
    public IActionResult ResetScoreboard()
    {
        return Ok(_store.ResetScoreboard());
    }
}
