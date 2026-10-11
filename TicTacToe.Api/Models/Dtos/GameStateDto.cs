public record GameStateDto(
    Guid Id,
    string Mode,
    List<Player?> Board,
    string CurrentPlayer,
    string Status,
    string? Winner,
    List<int> WinningCells,
    List<MoveDto> Moves,
    ScoreboardDto Scoreboard
);
