public sealed class GameSession
{
    public Guid Id { get; init; }
    public GameMode Mode { get; init; }
    public List<Player?> Board { get; set; } = Enumerable.Repeat<Player?>(null, 9).ToList();
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameStatus Status { get; set; } = GameStatus.Playing;
    public Player? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new();
    public List<GameMove> Moves { get; } = new();
    public bool ScoreRecorded { get; set; }
    public object SyncRoot { get; } = new();
}
