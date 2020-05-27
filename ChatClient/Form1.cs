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
using System.Windows.Input;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private string IP = "127.0.0.1";

        private string Nickname;
        string readData = null;
        public Form1()
        {
            InitializeComponent();

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            ConnectToServer();
        }

        private void Exit()
        {
            SendString("/exit");
            _clientSocket.Disconnect(false);
            _clientSocket.Dispose();
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectButton.Enabled = true;
            disconnectButton.Enabled = false;
            ipTextBox.Enabled = true;
            nicknameTextBox.Enabled = true;
            SendButton.Enabled = false;
            requestTextBox.Enabled = false;
        }

        private void SendString(string v)
        {
            try
            {
                byte[] _buffer = Encoding.UTF8.GetBytes(v);
                _clientSocket.Send(_buffer, 0, _buffer.Length, SocketFlags.None);
            }
            catch(Exception)
            {
                return;
            }
        }

        private void ConnectToServer()
        {
            int attempts = 0;
            if (Nickname != null && Nickname.Length >= 2 && !Nickname.Contains(":"))
            {
                while (!_clientSocket.Connected)
                { 
                    try
                    {
                        //try to connect
                        attempts++;
                        _clientSocket.Connect(IPAddress.Parse(IP), 5555);
                        disconnectButton.Enabled = true;
                        ipTextBox.Enabled = false;
                        connectButton.Enabled = false;
                        nicknameTextBox.Enabled = false;
                        SendButton.Enabled = true;
                        requestTextBox.Enabled = true;
                        Thread cT = new Thread(ReceiveResponse);
                        cT.Start();
                    }
                    catch (SocketException)
                    {
                        return;
                    }
                    catch (Exception)
                    { 
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Your nickname cannot be < 2 characters!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (requestTextBox.Text != "" && requestTextBox.Text != " ")
            {
                SendRequest();
            }
        }

        private void ReceiveResponse()
        {
            while(true)
            {
                try
                {
                    var _buffer = new byte[1024];
                    int received = _clientSocket.Receive(_buffer, SocketFlags.None);
                    if (received == 0)
                        return;
                    var data = new byte[received];
                    Array.Copy(_buffer, data, received);
                    string text = Encoding.UTF8.GetString(data);
                    readData = text;
                    Msg();
                }
                catch(SocketException)
                {
                    return;
                }
            }
        }

        private void Msg()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(Msg));
            else
                chatBox.Text += DateTime.Now.ToLongTimeString() + "  " + readData + "\r\n";
        }

        private void SendRequest()
        {
            string request =$"{Nickname}: {requestTextBox.Text}";
            SendString(request);

            if(request.ToLower() == "/exit")
            {
                Exit();
            }
        }

        private void ipTextBox_TextChanged(object sender, EventArgs e)
        {
            IP = ipTextBox.Text;
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                Exit();
                MessageBox.Show("Disconnected");
                connectButton.Enabled = true;
                disconnectButton.Enabled = false;
            }
            catch(SocketException err)
            {
                MessageBox.Show(err.ToString());
                return;
            }
        }

        private void nicknameTextBox_TextChanged(object sender, EventArgs e)
        {
            Nickname = nicknameTextBox.Text;
        }

        private void requestTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 0xD)
            {
                if(requestTextBox.Text != "" && requestTextBox.Text != " ")
                {
                    e.Handled = true;
                    SendButton.PerformClick();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Exit();
            }
            catch(Exception)
            {
                Environment.Exit(0);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MaximizeBox = false;
        }
    }
}
