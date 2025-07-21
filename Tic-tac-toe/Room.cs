using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace Tic_tac_toe
{
    public partial class Room : Form
    {
        private HubConnection _connection;
        public static Room Instance = null;
        public Room()
        {
            InitializeComponent();
            InitializeSignalR();
            Instance = this;
        }

        private async void InitializeSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7108/roomhub") // Adjust URL/port as needed
                .WithAutomaticReconnect()
                .Build();

            // Handle server events
            _connection.On<IEnumerable<RoomInfo>>("RoomsListed", rooms =>
            {
                Invoke(new Action(() =>
                {
                    listBox1.Items.Clear();
                    foreach (var room in rooms)
                        listBox1.Items.Add(room);
                }));
            });

            _connection.On<string>("RoomCreated", roomName =>
            {
                Invoke(new Action(() =>
                {
                    listBox1.Items.Add(roomName);
                }));
            });

            _connection.On<string>("Error", message =>
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });

            await _connection.StartAsync();
        }

        private async void buttonListRooms_Click(object sender, EventArgs e)
        {
            if (_connection.State == HubConnectionState.Connected)
                await _connection.InvokeAsync("ListRooms");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_connection.State == HubConnectionState.Connected)
                await _connection.InvokeAsync("CreateRoom");
        }

        private async void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is RoomInfo roomInfo && _connection.State == HubConnectionState.Connected)
            {
                if (roomInfo.MemberCount <= 1)
                {
                    Tic_tac_toe game = new Tic_tac_toe(roomInfo.RoomName);
                    game.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Room is full.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optional: handle selection change if needed
        }
    }
}