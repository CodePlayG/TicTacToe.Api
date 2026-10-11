using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace TicTacToe.Api.Tests
{
    public class ScoreboardControllerTests
    {
        [Fact]
        public void GetScoreboard_ReturnsOkResult()
        {
            // Arrange
            var mockStore = new Mock<GameSessionStore>();
            mockStore.Setup(store => store.ScoreboardSnapshot()).Returns(new ScoreboardDto(0, 0, 0));
            var controller = new ScoreboardController(mockStore.Object);

            // Act
            var result = controller.GetScoreboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void ResetScoreboard_ReturnsOkResult()
        {
            // Arrange
            var mockStore = new Mock<GameSessionStore>();
            mockStore.Setup(store => store.ResetScoreboard()).Returns(new ScoreboardDto(0, 0, 0));
            var controller = new ScoreboardController(mockStore.Object);

            // Act
            var result = controller.ResetScoreboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}