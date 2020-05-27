using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;
using System.Net;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.SqlServer.Server;
using System.Security.Cryptography;
using System.Net.Configuration;

namespace Server
{
    class Program
    {
        private static readonly Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> _clientSockets = new List<Socket>();

        private static bool debugging = false;

        private static List<string> _users = new List<string>();
        private const int _buffer_size = 1024;
        private const int _port = 5555;
        private static readonly byte[] _buffer = new byte[_buffer_size];

        static void Main()
        {
            Console.Title = "Remote Host";
            ServerSetup();
            Console.ReadLine();
            CloseAllSockets();

        }

        private static void CloseAllSockets()
        {
            foreach(Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            _serverSocket.Close();
        }

        private static void ServerSetup()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _serverSocket.Listen(0);
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup has been completed.");

        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket;
            try
            {
                socket = _serverSocket.EndAccept(ar);
            }
            catch(ObjectDisposedException)
            {
                return;
            }

            _clientSockets.Add(socket);
            socket.BeginReceive(_buffer, 0, 1024, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for requests...");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket current = (Socket)ar.AsyncState;

            int received;

            try
            {
                received = current.EndReceive(ar);

            }
            catch(SocketException)
            {
                current.Close();
                _clientSockets.Remove(current);
                return;
            }
            byte[] _receivingBuffer = new byte[received];
            Array.Copy(_buffer, _receivingBuffer, received);
            string text = Encoding.UTF8.GetString(_receivingBuffer);
            Console.WriteLine(text);
            string u_name = "";
            if(text != null && text.IndexOf(":") != -1)
                u_name = text.Substring(0, text.IndexOf(":"));
            try
            {
                if (!_users.Contains(u_name))
                {
                    _users.Add(u_name);
                    if (debugging)
                        foreach (string u in _users)
                        {
                            Console.WriteLine(u);
                        }
                }
            }
            catch(Exception)
            {

            }

            if(text.ToLower() == "/exit")
            {
                if (_users.Contains(u_name))
                    _users.Remove(u_name);
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                _clientSockets.Remove(current);
                Console.WriteLine("Client disconnected!");
                if(debugging)
                    foreach (string u in _users)
                    {
                        Console.WriteLine(u);
                    }
                return;
            }
            else
            {
                foreach(Socket client in _clientSockets)
                {
                    byte[] data = Encoding.UTF8.GetBytes(text);
                    client.Send(data);
                }
            }

            current.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, current);
        }
    }
}