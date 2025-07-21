using System.Collections.Concurrent;

namespace Server
{
    public class RoomGameManager
    {
        public ConcurrentDictionary<string, int> RoomMembers { get; } = new();
        // RoomName -> Game State
        public ConcurrentDictionary<string, GameState> Games { get; } = new();

        public EventHandler<string> RoomChange;
    }

    public class GameState
    {
        public int[,] Board { get; set; } = new int[15, 15];
        public bool Turn { get; set; } = false;
        public string[] PlayerIds { get; set; } = new string[2];
        public int PlayerCount { get; set; } = 0;
        public int[] Wins { get; set; } = new int[2]; // Wins[0] for PlayerIds[0], Wins[1] for PlayerIds[1]
        public int Ties { get; set; } = 0;
        public HashSet<string> RestartRequests { get; set; } = new HashSet<string>();
        public bool IsXTurn { get; set; } = true; // true: X starts, false: O starts
    }
}
