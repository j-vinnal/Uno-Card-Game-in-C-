# UNO Card Game

This project is a C# implementation of the UNO card game. The game supports both console and web-based play, with features including game state saving/loading, AI players, and multiplayer support. 
**Work is still in progress.**


## Project Overview

The UNO Card Game project is structured into several components:

- **ConsoleApp**: The console-based application for playing UNO.
- **DAL**: Data Access Layer, including Entity Framework context and repositories.
- **Domain**: Domain models and enums representing the game entities.
- **GameEngine**: Core game logic and rules implementation.
- **Helpers**: Utility classes and methods.
- **MenuSystem**: Console menu system.
- **UnoConsoleUI**: Console UI components.
- **WebApp**: Web application using Razor Pages for playing UNO online.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

### Installation

1. Clone the repository:
    ```bash
    git clone <repository-url>
    cd uno-card-game
    ```

2. Update .NET tools:
    ```bash
    dotnet tool update --global dotnet-ef
    dotnet tool update --global dotnet-aspnet-codegenerator
    ```

### Database Setup

1. Create a new migration:
    ```bash
    dotnet ef migrations add --project DAL --startup-project ConsoleApp InitialCreate
    ```
    or
    ```bash
    dotnet ef migrations add --project DAL --startup-project WebApp InitialCreate
    ```

2. Update the database:
    ```bash
    dotnet ef database update --project DAL --startup-project WebApp
    ```

### Running the Console App

Navigate to the `ConsoleApp` directory and run:
```bash
dotnet run
```

### Running the Web App

Navigate to the `WebApp` directory and run:
```bash
dotnet run
```

## Changing Data Repository

If you want to switch between using a database or JSON file repository, you need to update the `Program.cs` files in both the Web App and Console App.

### Web App

In the Web App, modify the [`Program.cs`](./WebApp/Program.cs) file located in `WebApp/Program.cs`:

```csharp
builder.Services.AddScoped<IGameRepository, GameRepositoryEF>();
//builder.Services.AddSingleton<IGameRepository, GameRepositoryFileSystem>();
```

Comment out the line for the repository you do not need.

### Console App

In the Console App, modify the [`Program.cs`](./ConsoleApp/Program.cs) file located in `ConsoleApp/Program.cs`:

```csharp
//IGameRepository gameRepository = new GameRepositoryFileSystem();
IGameRepository gameRepository = new GameRepositoryEF(db);
```

Comment out the line for the repository you do not need.

## Game Logic

### Game Setup
The game setup involves initializing the game options, setting up players, and dealing cards. This is handled by the [`UnoGameEngine`](./GameEngine/UnoGameEngine.cs) class.

### Game Flow in Console
The game flow is managed by the [`GameController`](./UnoConsoleUI/GameController.cs) class, which handles player turns, card plays, and game state updates.

### AI Players
AI players are supported and can be configured to play automatically. The AI logic is implemented within the [`UnoGameEngine`](./GameEngine/UnoGameEngine.cs) class.

### Saving and Loading Game State
The game state can be saved to and loaded from both JSON files and a database. The data access logic is implemented in the [`GameRepositoryFileSystem`](./DAL/GameRepositoryFileSystem.cs) and [`GameRepositoryEF`](./DAL/GameRepositoryEF.cs) classes.

### License
This project is licensed under the MIT License.

### Acknowledgments
[UNO (card game) on Wikipedia](https://en.wikipedia.org/wiki/Uno_(card_game))