public interface IGameSessionStore
{
    GameSession Create(GameMode mode);
    GameStateDto ToDto(GameSession game);
    ScoreboardDto ScoreboardSnapshot();
    bool TryGet(Guid id, out GameSession game);
    string? PlayMove(GameSession game, int position);
    void Undo(GameSession game);
    void ResetGame(GameSession game);
    ScoreboardDto ResetScoreboard();
}