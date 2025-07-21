using Microsoft.AspNetCore.SignalR;
using Server;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Common;

public class RoomHub : Hub<IRoomManager>
{
    private readonly RoomGameManager _manager;
    public RoomHub(RoomGameManager manager)
    {
        _manager = manager;
        _manager.RoomChange += Manager_RoomChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _manager.RoomChange -= Manager_RoomChanged;
        }
        base.Dispose(disposing);
    }

    private async void Manager_RoomChanged(object? sender, string roomName)
    {
        if (_manager.RoomMembers.TryGetValue(roomName, out var members) && _manager.Games.TryGetValue(roomName, out var game))
        {
            _manager.RoomMembers[roomName] = game.PlayerCount;

            await Clients.All.RoomsListed(GetRoomInfos());
        }
    }

    private List<RoomInfo> GetRoomInfos()
    {
        return _manager.RoomMembers
            .Select(kvp => new RoomInfo { RoomName = kvp.Key, MemberCount = kvp.Value })
            .ToList();
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.RoomsListed(GetRoomInfos());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateRoom()
    {
        string roomName = $"Room_{Guid.NewGuid()}";
        if (_manager.RoomMembers.TryAdd(roomName, 0))
        {
            await Clients.All.RoomCreated(roomName);
            await Clients.All.RoomsListed(GetRoomInfos());
        }
        else
        {
            await Clients.Caller.Error("Room already exists.");
        }
    }

    public async Task JoinRoom(string roomName)
    {
        if (_manager.RoomMembers.TryGetValue(roomName, out var members))
        {
            await Clients.All.RoomsListed(GetRoomInfos());
        }
        else
        {
            await Clients.Caller.Error("Room does not exist.");
        }
    }
}