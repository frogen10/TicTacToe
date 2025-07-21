using Common;

namespace Server
{
    public interface IRoomManager
    {
        Task RoomCreated(string roomName);
        Task Error(string message);
        Task UserJoined(string userId);
        Task UserLeft(string userId);
        Task ReceiveMessage(string userId, string message);
        Task RoomsListed(IEnumerable<RoomInfo> rooms);
    }
}