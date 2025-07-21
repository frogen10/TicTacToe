namespace Server
{
    public interface IGameClient
    {
        Task PlayerAssigned(string symbol, string clientId);
        Task GameFull();
        Task MoveMade(int row, int col, int player);
        Task GameOver(int player);
        Task GameStarted();
        Task RestartRequested(string userId);
        Task Restart(string ClientId);
        Task ScoreUpdated(int v1, int v2, int ties);
    }
}