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
    public static class Helpers
    {
        public static class Server
        {
            public static void SendRequest(string text)
            {
                string request = $"{Client.Nickname}: {text}";
                SendString(request);

                if (request.ToLower() == "/exit")
                {
                    Exit();
                }
            }


            public static void ReceiveResponse()
            {
                while (true)
                {
                    try
                    {
                        var _buffer = new byte[1024];
                        int received = Client._clientSocket.Receive(_buffer, SocketFlags.None);
                        if (received == 0)
                            return;
                        var data = new byte[received];
                        Array.Copy(_buffer, data, received);
                        string text = Encoding.ASCII.GetString(data);
                        Client.readData = text;
                        msg();
                    }
                    catch (SocketException)
                    {
                        return;
                    }
                }
            }

            public static void msg()
            {
                Client.TxtBox+= DateTime.Now.ToLongTimeString() + "  " + Client.readData + "\r\n";
            }

            public static void Exit()
            {
                SendString("/exit");
                Client._clientSocket.Disconnect(false);
                Client._clientSocket.Dispose();
                Client._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            public static void SendString(string v)
            {
                try
                {
                    byte[] _buffer = Encoding.ASCII.GetBytes(v);
                    Client._clientSocket.Send(_buffer, 0, _buffer.Length, SocketFlags.None);
                }
                catch (Exception)
                {
                }
            }

            public static void ConnectToServer()
            {
                if (!Client._clientSocket.Connected)
                {
                    int attempts = 0;
                    if (Client.Nickname != null && Client.Nickname.Length >= 2 && !Client.Nickname.Contains(":"))
                    {
                        try
                        {
                            while (!Client._clientSocket.Connected)
                            {
                                //try to connect
                                attempts++;
                                Client._clientSocket.Connect(IPAddress.Parse(Client.IP), 5555);
                                Thread cT = new Thread(() => ReceiveResponse());
                                cT.Start();
                            }
                        }
                        catch { };
                    }
                    else
                    {
                        MessageBox.Show("Nieprawidłowy nick.", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    try
                    {
                        Exit();
                    }
                    catch (SocketException err)
                    {
                        MessageBox.Show(err.ToString());
                        return;
                    }
                }
            }
        }
    }
}
