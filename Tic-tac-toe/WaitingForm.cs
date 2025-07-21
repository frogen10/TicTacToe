using System;
using System.Windows.Forms;

namespace Tic_tac_toe
{
    public partial class WaitingForm : Form
    {
        public WaitingForm()
        {
            InitializeComponent();
            this.ControlBox = false; // Removes close, minimize, maximize
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Waiting";
            var label = new Label
            {
                Text = "Waiting for opponent...",
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 14),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(label);
        }
    }
}