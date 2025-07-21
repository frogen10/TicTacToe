# TicTacToe

## Overview

This is a client-server TicTacToe game built with C# and .NET. The project uses ASP.NET Core SignalR for real-time communication between the server and multiple clients. Players can join rooms, play TicTacToe, and see live updates as moves are made.

## Features

- Real-time multiplayer TicTacToe using SignalR
- Room creation and management
- Automatic assignment of X and O to players
- Score tracking (wins, losses, ties)
- Game restart and leave functionality
- Win/tie detection and notifications
- Modern WinForms client UI

## Project Structure

- `Server/` - ASP.NET Core backend with SignalR hubs for game and room management
- `Tic-tac-toe/` - WinForms client application
- `Common/` - Shared models and interfaces

## How It Works

1. **Server**: Hosts SignalR hubs (`GameHub`, `RoomHub`) to manage rooms and game state.
2. **Client**: Connects to the server, lists available rooms, allows users to create or join rooms, and plays the game in real time.
3. **Communication**: All moves, room changes, and game events are sent via SignalR, ensuring instant updates for all connected clients.

## Getting Started

### Prerequisites

- .NET 7.0 or later (for both server and client)
- Visual Studio 2022+ or VS Code

### Build and Run the Server

1. Open the `Server/` folder in Visual Studio or VS Code.
2. Restore NuGet packages if needed.
3. Run the project. The server will start and listen for SignalR connections (default: `https://localhost:7108`).

### Build and Run the Client

1. Open the `Tic-tac-toe/` folder in Visual Studio.
2. Build and run the WinForms application.
3. The client will connect to the server at `https://localhost:7108` (adjust the URL in code if needed).

### Playing the Game

1. Start the server.
2. Launch one or more clients.
3. Create or join a room from the client UI.
4. When two players are in a room, the game starts automatically.
5. Play by clicking on the board. The server validates moves and updates both clients in real time.
6. When the game ends (win/tie/leave), you can restart or leave the room.

## Customization

- To change the board size or win condition, modify the logic in `GameHub.cs` and `GameState`.
- To deploy to production, update the server URL and configure HTTPS certificates as needed.

## Technologies Used

- C# / .NET 7+
- ASP.NET Core SignalR
- WinForms
- Concurrent collections for thread-safe game state

## Credits

Created by Lucas

---
Feel free to contribute or open issues for suggestions and bug reports!