# Architecture and Flow

## Architecture

### Components:
1. **GameController (Controller Layer):**
   - Handles HTTP requests and returns appropriate HTTP responses.
   - Contains methods like `PlayMove`, `CreateGame`, `GetGame`, `UndoMove`, and `ResetGame`.

2. **IGameSessionStore (Service/Repository Layer):**
   - Interface for managing game sessions.
   - Provides methods like `TryGet`, `Create`, `PlayMove`, and `ToDto`.

3. **GameSession (Model Layer):**
   - Represents the state of a game session.
   - Contains properties like `Id`, `Mode`, `Board`, `CurrentPlayer`, `Status`, etc.

4. **DTOs (Data Transfer Objects):**
   - `GameStateDto`, `MoveDto`, `ScoreboardDto`: Used to transfer data between layers.

5. **Mocking Framework (Moq):**
   - Used in tests to simulate the behavior of `IGameSessionStore`.

### Relationships:
- The `GameController` depends on `IGameSessionStore` for game session operations.
- `IGameSessionStore` interacts with `GameSession` and DTOs to manage and return game data.

---

## Flow

### PlayMove_InvalidMove_ReturnsBadRequest Test Flow:
1. **Arrange:**
   - A mock `IGameSessionStore` is created.
   - A `GameSession` object is initialized with default values.
   - Mock behavior is set up:
     - `TryGet` returns the `GameSession`.
     - `PlayMove` returns an error message for invalid moves.
   - A `GameController` instance is created with the mock store.

2. **Act:**
   - The `PlayMove` method of `GameController` is called with an invalid move request.

3. **Assert:**
   - The result is checked to ensure it is a `BadRequestObjectResult`.
   - The error message in the response is validated.

---

### CreateGame_ValidMode_ReturnsCreatedResult Test Flow:
1. **Arrange:**
   - A mock `IGameSessionStore` is created.
   - A `GameSession` object is initialized with a valid mode.
   - Mock behavior is set up:
     - `Create` returns the `GameSession`.
     - `ToDto` converts the `GameSession` to a `GameStateDto`.
   - A `GameController` instance is created with the mock store.

2. **Act:**
   - The `CreateGame` method of `GameController` is called with a valid mode request.

3. **Assert:**
   - The result is checked to ensure it is a `CreatedResult`.
   - The location and value of the response are validated.

---

### GetGame_NotFound_ReturnsNotFound Test Flow:
1. **Arrange:**
   - A mock `IGameSessionStore` is created.
   - Mock behavior is set up:
     - `TryGet` returns `false` for the given game ID.
   - A `GameController` instance is created with the mock store.

2. **Act:**
   - The `GetGame` method of `GameController` is called with a non-existent game ID.

3. **Assert:**
   - The result is checked to ensure it is a `NotFoundObjectResult`.

---

### UndoMove_ValidGame_ReturnsOkResult Test Flow:
1. **Arrange:**
   - A mock `IGameSessionStore` is created.
   - A `GameSession` object is initialized.
   - Mock behavior is set up:
     - `TryGet` returns the `GameSession`.
     - `ToDto` converts the `GameSession` to a `GameStateDto`.
   - A `GameController` instance is created with the mock store.

2. **Act:**
   - The `UndoMove` method of `GameController` is called with a valid game ID.

3. **Assert:**
   - The result is checked to ensure it is an `OkObjectResult`.
   - The value of the response is validated.

---

### ResetGame_ValidGame_ReturnsOkResult Test Flow:
1. **Arrange:**
   - A mock `IGameSessionStore` is created.
   - A `GameSession` object is initialized.
   - Mock behavior is set up:
     - `TryGet` returns the `GameSession`.
     - `ToDto` converts the `GameSession` to a `GameStateDto`.
   - A `GameController` instance is created with the mock store.

2. **Act:**
   - The `ResetGame` method of `GameController` is called with a valid game ID.

3. **Assert:**
   - The result is checked to ensure it is an `OkObjectResult`.
   - The value of the response is validated.