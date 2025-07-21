using Microsoft.AspNetCore.SignalR;

namespace Server
{
    public class GameHub : Hub<IGameClient>
    {
        private readonly RoomGameManager _manager;
        public GameHub(RoomGameManager manager)
        {
            _manager = manager;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Find the room(s) where this user is a player
            var rooms = _manager.Games
                .Where(kvp => kvp.Value.PlayerIds.Contains(Context.ConnectionId))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var roomName in rooms)
            {
                if (_manager.Games.TryGetValue(roomName, out var game))
                {
                    // Remove the player from the game
                    for (int i = 0; i < game.PlayerIds.Length; i++)
                    {
                        if (game.PlayerIds[i] == Context.ConnectionId)
                        {
                            game.PlayerIds[i] = null;
                            game.PlayerCount--;
                        }
                    }

                    // Notify the group and remove the game
                    await Clients.Group(roomName).GameOver(-999); // -999: special code for disconnect/game stopped
                    _manager.Games.TryRemove(roomName, out _);
                    _manager.RoomChange?.Invoke(this, roomName);
                    // Optionally, remove from SignalR group
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> JoinGame(string roomName)
        {
            if (!_manager.Games.ContainsKey(roomName))
            {
                _manager.Games[roomName] = new GameState();
            }
            var game = _manager.Games[roomName];

            if (game.PlayerCount < 2)
            {
                game.PlayerIds[game.PlayerCount] = Context.ConnectionId;
                game.PlayerCount++;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                await Clients.Caller.PlayerAssigned(game.PlayerCount == 1 ? "X" : "O", Context.ConnectionId);

                // Notify both players when the second joins
                if (game.PlayerCount == 2)
                {
                    await Clients.Group(roomName).GameStarted();
                }
                _manager.RoomChange?.Invoke(this, roomName);
                return true;
            }
            else
            {
                await Clients.Caller.GameFull();
                return false;
            }
        }

        public async Task LeaveGame(string roomName)
        {
            if (_manager.Games.TryGetValue(roomName, out var game))
            {
                // Remove the player from the game
                for (int i = 0; i < game.PlayerIds.Length; i++)
                {
                    if (game.PlayerIds[i] == Context.ConnectionId)
                    {
                        game.PlayerIds[i] = null;
                        game.PlayerCount--;
                    }
                }

                // Notify the group and remove the game
                await Clients.Group(roomName).GameOver(-999); // -999: special code for player left/game stopped
                _manager.RoomChange?.Invoke(this, roomName);

                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            }
        }

        public async Task<bool> RestartGame(string roomName)
        {
            if (_manager.Games.TryGetValue(roomName, out var game))
            {
                lock (game)
                {
                    game.RestartRequests.Add(Context.ConnectionId);
                }

                if (game.PlayerIds.All(pid => pid != null && game.RestartRequests.Contains(pid)))
                {
                    game.Board = new int[15, 15];
                    // Alternate the starting player
                    game.IsXTurn = !game.IsXTurn;
                    game.Turn = !game.IsXTurn; // false: X's turn, true: O's turn
                    game.RestartRequests.Clear();
                    string startingPlayerId = game.IsXTurn ? game.PlayerIds[0] : game.PlayerIds[1];
                    await Clients.Group(roomName).Restart(startingPlayerId);
                    return true;
                }
                else
                {
                    await Clients.Group(roomName).RestartRequested(Context.ConnectionId);
                }
                return false;
            }
            return false;
        }

        public async Task<bool> MakeMove(string roomName, int row, int col)
        {
            if (!_manager.Games.TryGetValue(roomName, out var game))
                return false;

            int playerIndex = (Context.ConnectionId == game.PlayerIds[0]) ? 0 : 1;
            int player = playerIndex == 0 ? -1 : 1;
            if (game.Board[row, col] != 0)
                return false;

            if ((game.Turn == false && player == -1) || (game.Turn == true && player == 1))
            {
                game.Board[row, col] = player;
                game.Turn = !game.Turn;

                bool winner = CheckForWinner(game.Board, row, col, player);
                await Clients.Group(roomName).MoveMade(row, col, player);

                if (winner)
                {
                    game.Wins[playerIndex]++;
                    await Clients.Group(roomName).GameOver(player);
                    // Optionally, send updated scores:
                    await Clients.Group(roomName).ScoreUpdated(game.Wins[0], game.Wins[1], game.Ties);
                }
                else if (IsBoardFull(game.Board))
                {
                    game.Ties++;
                    await Clients.Group(roomName).GameOver(0); // 0 or another code for tie
                    await Clients.Group(roomName).ScoreUpdated(game.Wins[0], game.Wins[1], game.Ties);
                }
                return true;
            }
            return false;
        }

        private bool IsBoardFull(int[,] board)
        {
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                    if (board[i, j] == 0)
                        return false;
            return true;
        }

        private bool CheckForWinner(int[,] board, int row, int col, int player)
        {
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 }, new int[] { 1, 0 },
                new int[] { 1, 1 }, new int[] { 1, -1 }
            };

            foreach (var dir in directions)
            {
                int count = 1;
                for (int d = 1; d < 5; d++)
                {
                    int r = row + dir[0] * d, c = col + dir[1] * d;
                    if (r < 0 || r >= 15 || c < 0 || c >= 15 || board[r, c] != player) break;
                    count++;
                }
                for (int d = 1; d < 5; d++)
                {
                    int r = row - dir[0] * d, c = col - dir[1] * d;
                    if (r < 0 || r >= 15 || c < 0 || c >= 15 || board[r, c] != player) break;
                    count++;
                }
                if (count >= 5) return true;
            }
            return false;
        }
    }
}