using System.Collections.Concurrent;

public sealed class GameSessionStore
{
    private static readonly int[][] WinningPatterns =
    {
        new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8},
        new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8},
        new[] {0, 4, 8}, new[] {2, 4, 6},
    };

    private readonly ConcurrentDictionary<Guid, GameSession> games = new();
    private readonly object scoreboardLock = new();
    private Scoreboard scoreboard = new();
    public GameSession Create(GameMode mode)
    {
        var game = new GameSession
        {
            Id = Guid.NewGuid(),
            Mode = mode,
            Board = Enumerable.Repeat<Player?>(null, 9).ToList(),
            CurrentPlayer = Player.X,
            Status = GameStatus.Playing,
            Winner = null,
            WinningCells = new List<int>(),
            ScoreRecorded = false
        };

        games[game.Id] = game;
        return game;
    }
    public GameStateDto ToDto(GameSession game)
    {
        return new GameStateDto(
            game.Id,
            game.Mode == GameMode.TwoPlayer ? "two-player" : "computer",
            game.Board,
            game.CurrentPlayer.ToString(),
            game.Status.ToString().ToLowerInvariant(),
            game.Winner?.ToString(),
            game.WinningCells,
            game.Moves.Select(move => new MoveDto(move.Player.ToString(), move.Position)).ToList(),
            ScoreboardSnapshot()
        );
    }
    public ScoreboardDto ScoreboardSnapshot()
    {
        lock (scoreboardLock)
        {
            return new ScoreboardDto(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);
        }
    }
    public bool TryGet(Guid id, out GameSession game)
    {
        return games.TryGetValue(id, out game!);
    }
    public string? PlayMove(GameSession game, int position)
    {
        // Validate the move
        if (game.Status != GameStatus.Playing) return "This game is already complete.";
        if (position < 0 || position > 8) return "Position must be between 0 and 8.";
        if (game.Board[position] is not null) return "That cell is already occupied.";
        if (game.Mode == GameMode.Computer && game.CurrentPlayer != Player.X) return "Wait for the computer's turn to complete.";

        // Place the player's move
        PlaceMove(game, position, game.CurrentPlayer);

        // If the game mode is Computer and the game is still ongoing, let the computer play
        if (game.Mode == GameMode.Computer && game.Status == GameStatus.Playing)
        {
            var computerPosition = ChooseComputerMove(game.Board);
            if (computerPosition is not null)
            {
                PlaceMove(game, computerPosition.Value, Player.O);
            }
        }

        return null; // No errors
    }
    private void PlaceMove(GameSession game, int position, Player player)
    {
        game.Board[position] = player;
        game.Moves.Add(new GameMove(player, position));

        // Check for a win
        var winningCells = FindWinningCells(game.Board, player);
        if (winningCells is not null)
        {
            game.Status = GameStatus.Won;
            game.Winner = player;
            game.WinningCells = winningCells.ToList();
            RecordResult(game, player);
            return;
        }

        // Check for a draw
        if (game.Board.All(cell => cell is not null))
        {
            game.Status = GameStatus.Draw;
            RecordResult(game, null);
            return;
        }

        // Switch the current player
        game.CurrentPlayer = player == Player.X ? Player.O : Player.X;
    }
    private int? ChooseComputerMove(IReadOnlyList<Player?> board)
    {
        // Try to win
        var win = FindPotentialMove(board, Player.O);
        if (win is not null) return win;

        // Try to block the opponent
        var block = FindPotentialMove(board, Player.X);
        if (block is not null) return block;

        // Take the center if available
        if (board[4] is null) return 4;

        // Take a corner if available
        var corner = new[] { 0, 2, 6, 8 }.FirstOrDefault(pos => board[pos] is null, -1);
        if (corner != -1) return corner;

        // Take any available cell
        var cell = Enumerable.Range(0, 9).FirstOrDefault(pos => board[pos] is null, -1);
        return cell == -1 ? null : cell;
    }
    private static int[]? FindWinningCells(IReadOnlyList<Player?> board, Player player)
    {
        return WinningPatterns.FirstOrDefault(pattern => pattern.All(index => board[index] == player));
    }
    private static int? FindPotentialMove(IReadOnlyList<Player?> board, Player player)
    {
        foreach (var position in Enumerable.Range(0, 9).Where(index => board[index] is null))
        {
            var candidate = board.ToList();
            candidate[position] = player;
            if (FindWinningCells(candidate, player) is not null) return position;
        }
        return null;
    }
    private void RecordResult(GameSession game, Player? winner)
    {
        if (game.ScoreRecorded) return;

        lock (scoreboardLock)
        {
            if (winner == Player.X) scoreboard.XWins++;
            else if (winner == Player.O) scoreboard.OWins++;
            else scoreboard.Draws++;
        }

        game.ScoreRecorded = true;
    }
    public void Undo(GameSession game)
    {
        if (game.Moves.Count == 0) return; // No moves to undo

        // Revert the recorded result if the game was completed
        RevertRecordedResult(game);

        // Determine how many moves to undo
        var moveCount = game.Mode == GameMode.Computer ? Math.Min(2, game.Moves.Count) : 1;

        // Remove the last move(s)
        game.Moves.RemoveRange(game.Moves.Count - moveCount, moveCount);

        // Rebuild the game state from the remaining moves
        RebuildGame(game);
    }
    private void RevertRecordedResult(GameSession game)
    {
        if (!game.ScoreRecorded) return;

        lock (scoreboardLock)
        {
            if (game.Winner == Player.X) scoreboard.XWins = Math.Max(0, scoreboard.XWins - 1);
            else if (game.Winner == Player.O) scoreboard.OWins = Math.Max(0, scoreboard.OWins - 1);
            else if (game.Status == GameStatus.Draw) scoreboard.Draws = Math.Max(0, scoreboard.Draws - 1);
        }

        game.ScoreRecorded = false;
    }
    private void RebuildGame(GameSession game)
    {
        // Reset the game state
        game.Board = Enumerable.Repeat<Player?>(null, 9).ToList();
        game.CurrentPlayer = Player.X;
        game.Status = GameStatus.Playing;
        game.Winner = null;
        game.WinningCells = new List<int>();
        game.ScoreRecorded = false;

        // Replay all moves to rebuild the state
        foreach (var move in game.Moves)
        {
            game.Board[move.Position] = move.Player;
            game.CurrentPlayer = move.Player == Player.X ? Player.O : Player.X;
        }
    }
    public void ResetGame(GameSession game)
    {
        // Reset the game board to its initial state
        game.Board = Enumerable.Repeat<Player?>(null, 9).ToList();

        // Clear the move history
        game.Moves.Clear();

        // Reset the current player to Player.X
        game.CurrentPlayer = Player.X;

        // Reset the game status to Playing
        game.Status = GameStatus.Playing;

        // Clear the winner and winning cells
        game.Winner = null;
        game.WinningCells = new List<int>();

        // Mark the game as not having a recorded score
        game.ScoreRecorded = false;
    }
    public ScoreboardDto ResetScoreboard()
    {
        lock (scoreboardLock)
        {
            // Reset the scoreboard values
            scoreboard = new Scoreboard
            {
                XWins = 0,
                OWins = 0,
                Draws = 0
            };

            // Return the updated scoreboard as a DTO
            return new ScoreboardDto(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);
        }
    }
}
