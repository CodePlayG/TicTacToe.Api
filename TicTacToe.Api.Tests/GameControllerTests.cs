using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace TicTacToe.Api.Tests
{
    public class GameControllerTests
    {
        [Fact]
        public void CreateGame_InvalidMode_ReturnsBadRequest()
        {
            // Arrange
            var mockStore = new Mock<GameSessionStore>();
            var controller = new GameController(mockStore.Object);
            var request = new CreateGameRequest("invalid-mode"); 

            // Act
            var result = controller.CreateGame(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("mode must be two-player or computer.", ((dynamic)badRequestResult.Value).message);
        }

        [Fact]
        public void CreateGame_ValidMode_ReturnsCreatedResult()
        {
            // Arrange
            var mockStore = new Mock<GameSessionStore>();
            var game = new GameSession { Id = Guid.NewGuid() };
            mockStore.Setup(store => store.Create(It.IsAny<GameMode>())).Returns(game);
            mockStore.Setup(store => store.ToDto(game)).Returns(new GameStateDto(
     game.Id,
     game.Mode == GameMode.TwoPlayer ? "two-player" : "computer",
     game.Board,
     game.CurrentPlayer.ToString(),
     game.Status.ToString().ToLowerInvariant(),
     game.Winner?.ToString(),
     game.WinningCells,
     game.Moves.Select(move => new MoveDto(move.Player.ToString(), move.Position)).ToList(),
     mockStore.Object.ScoreboardSnapshot()
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
            var mockStore = new Mock<GameSessionStore>();
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
            var mockStore = new Mock<GameSessionStore>();
            var game = new GameSession();
            mockStore.Setup(store => store.TryGet(It.IsAny<Guid>(), out game)).Returns(true);
            //mockStore.Setup(store => store.ToDto(game)).Returns(new { });
            var controller = new GameController(mockStore.Object);

            // Act
            var result = controller.UndoMove(Guid.NewGuid());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void ResetGame_ValidGame_ReturnsOkResult()//TODO: Fix
        {
            // Arrange
            var mockStore = new Mock<GameSessionStore>();
            var game = new GameSession();

            // Properly set up the out parameter
            mockStore
                .Setup(store => store.TryGet(It.IsAny<Guid>(), out game))
                .Returns(true);

            // Return the correct DTO type instead of an anonymous object
            //mockStore
            //       .Setup(store => store.ToDto(It.IsAny<GameSession>()))
            //       .Returns(new GameSessionDto { Id = Guid.NewGuid() });

            var controller = new GameController(mockStore.Object);

            // Act
            var result = controller.ResetGame(Guid.NewGuid());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
           // Assert.IsType<GameSessionDto>(okResult.Value);
        }
    }
}