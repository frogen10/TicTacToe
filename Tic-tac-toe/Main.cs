using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace Tic_tac_toe
{
    public partial class Tic_tac_toe : Form
    {
        bool turn = false;//X
        bool start = false;
        int[] wins = new int[3];
        string[] P_name = new string[2];
        private string _roomName;
        private string _clientId;
        private HubConnection _connection;
        private WaitingForm _waitingForm;

        public Tic_tac_toe()
        {
            InitializeComponent();
            pen = new Pen(Brushes.Black);
        }
        public Tic_tac_toe(string roomName) : this()
        {
            _roomName = roomName;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            P_name[0] = "X";
            P_name[1] = "O";
            label2.Text = _roomName;
            _waitingForm = new WaitingForm();
            _waitingForm.Show(this);
            InitializeSignalR();
        }

        private async void InitializeSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7108/gamehub") // Adjust as needed
                .WithAutomaticReconnect()
                .Build();

            // Register handlers for IGameClient interface
            _connection.On<string, string>("PlayerAssigned", (symbol, clientId) =>
            {
                _clientId = clientId;
                Invoke(new Action(() => this.Text = $"You are: {symbol}"));
                if (symbol == "X")
                {
                    turn = true; // X starts first
                }
                else
                {
                    turn = false; // O waits for X
                }
            });

            _connection.On("GameFull", () =>
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Game is full.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }));
            });

            _connection.On<int, int, int>("MoveMade", (row, col, player) =>
            {
                Invoke(new Action(() =>
                {
                    // Update board UI
                    int tabIndex = GetTabIndexFromRowCol(row, col);
                    var pic = GetPictureBoxByTabIndex(tabIndex);
                    if (pic != null)
                    {
                        pic.BackgroundImage = player == -1 ? Properties.Resources.cross : Properties.Resources.circle;
                    }
                    turn = !turn;
                }));
            });

            _connection.On<int>("GameOver", player =>
            {
                try
                {
                    Invoke(new Action(() =>
                    {
                        display_gameover(player);
                    }));
                }
                catch (Exception) { }
            });

            _connection.On("GameStarted", () =>
            {
                Invoke(new Action(() =>
                {
                    start = true;
                    _waitingForm.Close();
                }));
            });

            _connection.On<string>("RestartRequested", (playerName) =>
            {
                Invoke(new Action(() =>
                {
                    if(playerName != _clientId)
                    {
                        DialogResult dialogResult = MessageBox.Show($"Player requested a restart. Do you want play again?", "Restart Requested", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResult == DialogResult.Yes)
                        {
                            _connection.InvokeAsync("RestartGame", _roomName);
                        }
                    }
                }));
            });

            _connection.On<string>("Restart", (playerName) =>
            {
                Invoke(new Action(() =>
                {
                    if (playerName != _clientId)
                    {
                        turn = false; // Reset turn
                    }
                    else
                    {
                        turn = true; // Reset turn for the player who initiated the restart
                    }
                    restart();
                }));
            });

            _connection.On <int, int, int>("ScoreUpdated", (win1, win2, ties) =>
            {
                wins[0] = win1;
                wins[1] = win2;
                wins[2] = ties;
                Invoke(new Action(() =>
                {
                    foreach (Control c in groupBox1.Controls)
                    {
                        c.Text = c.TabIndex switch
                        {
                            4 => wins[0].ToString(),
                            5 => wins[1].ToString(),
                            7 => wins[2].ToString(),
                            _ => c.Text,
                        };
                    }
                }));
            });

            await _connection.StartAsync();
            await _connection.InvokeAsync("JoinGame", _roomName);
        }

        private (int row, int col) GetBoardCoordinatesFromTabIndex(int tabIndex)
        {
            int row = tabIndex / 15;
            int col = tabIndex % 15;
            return (row, col);
        }

        private PictureBox GetPictureBoxByTabIndex(int tabIndex)
        {
            foreach (Control control in Controls)
            {
                if (control is PictureBox pb && pb.TabIndex == tabIndex)
                    return pb;
            }
            return null;
        }

        private int GetTabIndexFromRowCol(int row, int col)
        {
            return row * 15 + col;
        }

        private async void picture_Click(object sender, EventArgs e)
        {
            if (!turn)
            {
                MessageBox.Show("It's not your turn.");
                return;
            }
            if (!start)
            {
                MessageBox.Show("Please start the game first.");
                return;
            }
            PictureBox b = (PictureBox)sender;
            if (b.BackgroundImage == null && _connection != null && _connection.State == HubConnectionState.Connected)
            {
                var (row, col) = GetBoardCoordinatesFromTabIndex(b.TabIndex);
                await _connection.InvokeAsync("MakeMove", _roomName, row, col);
            }
        }


        private void display_gameover(int player)
        {
            if(player == -999)
            {
                MessageBox.Show("Game ended user left the room", "Gameover", MessageBoxButtons.OK, MessageBoxIcon.Information);
                restart();
                return;
            }
            if(player == 0)
            {
                MessageBox.Show("Game ended in a tie", "Gameover", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int tmp = player == -1 ? 0 : 1;
            string res = "Winner: " + P_name[tmp];
            MessageBox.Show(res, "Gameover");
        }

        private void restart()
        {
            foreach (Control c in Controls)
            {
                if (c is PictureBox)
                {
                    if (c.BackgroundImage != null)
                    {
                        c.BackgroundImage = null;
                    }
                }
            }
        }

        public void ShowMyDialogBox()
        {
            P_name[0] = Player1.Text;
            P_name[1] = Player2.Text;
            Name testDialog = new Name(P_name);

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (testDialog.ShowDialog(this) != DialogResult.OK)
            {
                P_name = testDialog.P_name;
            }
            testDialog.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ShowMyDialogBox();
            Player1.Text = P_name[0] + ":";
            Player2.Text = P_name[1] + ":";
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by Lucas");
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            _connection?.InvokeAsync("LeaveGame", _roomName);
            this.Close();
            if (!Room.Instance.IsDisposed)
            {
                Room.Instance?.Show();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if(!Room.Instance.IsDisposed)
            {
                Room.Instance?.Show();
            }
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            _connection?.InvokeAsync("RestartGame", _roomName);
        }
    }

    public partial class Name : Form
    {
        public string[] P_name = new string[2];
        public Name(string[] P)
        {
            InitializeComponent();
            textBox1.Text = P[0][..(P[0].Length-1)];
            textBox2.Text = P[1][..(P[0].Length - 1)];
        }
        private void Button_Click(object sender, EventArgs e)
        {
            P_name[0] = textBox1.Text;
            P_name[1] = textBox2.Text;
            this.Close();
        }
        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Button_Click(sender, e);
        }
    }
}