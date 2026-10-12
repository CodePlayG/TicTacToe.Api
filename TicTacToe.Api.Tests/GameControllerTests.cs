using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace TicTacToe.Api.Tests
{
    public class GameControllerTests
    {
        [Fact]
        public void PlayMove_InvalidMove_ReturnsBadRequest()
        {
            // Arrange
            var mockStore = new Mock<IGameSessionStore>();
            var game = new GameSession
            {
                Id = Guid.NewGuid(),
                Mode = GameMode.TwoPlayer,
                Board = Enumerable.Repeat<Player?>(null, 9).ToList(),
                CurrentPlayer = Player.X,
                Status = GameStatus.Playing
            };
            mockStore.Setup(store => store.TryGet(It.IsAny<Guid>(), out game)).Returns(true);
            mockStore.Setup(store => store.PlayMove(game, 10)).Returns("Position must be between 0 and 8.");
            var controller = new GameController(mockStore.Object);
            var request = new MoveRequest(10); // Invalid position

            // Act
            var result = controller.PlayMove(game.Id, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // var response = Assert.IsType(badRequestResult.Value);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Position must be between 0 and 8.", value?.message);
        }

        [Fact]
        public void CreateGame_ValidMode_ReturnsCreatedResult()
        {
            // Arrange
            var mockStore = new Mock<IGameSessionStore>();
            var game = new GameSession { Id = Guid.NewGuid(), Mode = GameMode.TwoPlayer };
            mockStore.Setup(store => store.Create(It.IsAny<GameMode>())).Returns(game);
            mockStore.Setup(store => store.ToDto(game)).Returns(new GameStateDto(
                game.Id,
                "two-player",
                game.Board,
                game.CurrentPlayer.ToString(),
                game.Status.ToString().ToLowerInvariant(),
                game.Winner?.ToString(),
                game.WinningCells,
                game.Moves.Select(move => new MoveDto(move.Player.ToString(), move.Position)).ToList(),
                new ScoreboardDto(3, 2, 1)
            ));
            var controller = new GameController(mockStore.Object);
            var request = new CreateGameRequest("two-player");

            // Act
            var result = controller.CreateGame(request);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal($"/api/games/{game.Id}", createdResult.Location);
            Assert.NotNull(createdResult.Value);
        }

        [Fact]
        public void GetGame_NotFound_ReturnsNotFound()
        {
            // Arrange
            var mockStore = new Mock<IGameSessionStore>();
            GameSession? gameSession = null;
            mockStore.Setup(store => store.TryGet(It.IsAny<Guid>(), out gameSession)).Returns(false);
            var controller = new GameController(mockStore.Object);

            // Act
            var result = controller.GetGame(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void UndoMove_ValidGame_ReturnsOkResult()
        {
            // Arrange
            var mockStore = new Mock<IGameSessionStore>();
            var game = new GameSession();
            mockStore.Setup(store => store.TryGet(It.IsAny<Guid>(), out game)).Returns(true);
            mockStore.Setup(store => store.ToDto(game)).Returns(new GameStateDto(
                game.Id,
                "two-player",
                game.Board,
                game.CurrentPlayer.ToString(),
                game.Status.ToString().ToLowerInvariant(),
                game.Winner?.ToString(),
                game.WinningCells,
                game.Moves.Select(move => new MoveDto(move.Player.ToString(), move.Position)).ToList(),
                new ScoreboardDto(3, 2, 1)
            ));
            var controller = new GameController(mockStore.Object);

            // Act
            var result = controller.UndoMove(Guid.NewGuid());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void ResetGame_ValidGame_ReturnsOkResult()
        {
            // Arrange
            var mockStore = new Mock<IGameSessionStore>();
            var game = new GameSession();
            mockStore.Setup(store => store.TryGet(It.IsAny<Guid>(), out game)).Returns(true);
            mockStore.Setup(store => store.ToDto(game)).Returns(new GameStateDto(
                game.Id,
                "two-player",
                game.Board,
                game.CurrentPlayer.ToString(),
                game.Status.ToString().ToLowerInvariant(),
                game.Winner?.ToString(),
                game.WinningCells,
                game.Moves.Select(move => new MoveDto(move.Player.ToString(), move.Position)).ToList(),
                new ScoreboardDto(3, 2, 1)
            ));
            var controller = new GameController(mockStore.Object);

            // Act
            var result = controller.ResetGame(Guid.NewGuid());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}