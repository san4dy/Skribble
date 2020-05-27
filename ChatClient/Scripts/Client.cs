using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace ChatClient
{
    public partial class Client : Form
    {
        public static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static string IP = "127.0.0.1";

        public static string Nickname, TxtBox;
        public static string readData = null;
        public Client()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (ipTextBox.Text != "") IP = ipTextBox.Text;
            Nickname = nicknameTextBox.Text;
            if (_clientSocket.Connected)
            {
                connectButton.Text = "Połącz";
                connectButton.Location = new Point(45, 221);
                connectButton.Size = new Size(114, 23);
                label1.Visible = true;
                label2.Visible = true;
                chatBox.Visible = false;
                requestTextBox.Visible = false;
                SendButton.Visible = false;
                nicknameTextBox.Visible = true;
                ipTextBox.Visible = true;
                logoPicture.Visible = true;
            }
            else
            {
                connectButton.Text = "Rozłącz";
                connectButton.Location = new Point(718, 12);
                connectButton.Size = new Size(70, 23);
                label1.Visible = false;
                label2.Visible = false;
                chatBox.Visible = true;
                requestTextBox.Visible = true;
                SendButton.Visible = true;
                nicknameTextBox.Visible = false;
                ipTextBox.Visible = false;
                logoPicture.Visible = false;

            }
            Helpers.Server.ConnectToServer();

        }

        private void requestTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0xD)
            {
                if (requestTextBox.Text != "" && requestTextBox.Text != " ")
                {
                    e.Handled = true;
                    SendButton.PerformClick();
                }
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            Helpers.Server.Exit();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            Helpers.Server.SendRequest(requestTextBox.Text);
            Thread.Sleep(50);
            chatBox.Text = TxtBox;
        }
    }
}