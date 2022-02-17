using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace Hnefatafl
{
    sealed class Server
    {
        private NetServer _server;
        private List<NetPeer> _clients; 

        public void StartServer()
        {
            var config = new NetPeerConfiguration("Hnefatafl") { Port = 14242 };
            _server = new NetServer(config);
            _server.Start();

            if (_server.Status == NetPeerStatus.Running)
            {
                Console.WriteLine("Server is running on port " + config.Port);
            }
            else
            {
                Console.WriteLine("Server not started...");
            }
            _clients = new List<NetPeer>();
        }

        public void StopServer()
        {
            _clients.Clear();
            _server.Shutdown("bye");
            Console.WriteLine("Server closed");
        }

        public int ConnectionAmount()
        {
            return _clients.Count;
        }

        public void ReadMessages()
        {
            NetIncomingMessage message;
            
            while ((message = _server.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            var data = message.ReadString();
                            _server.SendMessage(_server.CreateMessage(data), _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                            break;
                        }
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine(message.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine(message.SenderConnection.Status);
                        if (message.SenderConnection.Status == NetConnectionStatus.Connected && _clients.Count < 2)
                        {
                            _clients.Add(message.SenderConnection.Peer);
                            if (_clients.Count == 2)
                            {
                                _server.SendMessage(_server.CreateMessage("false"), _server.Connections[1], NetDeliveryMethod.ReliableOrdered, 0);
                            }
                            Console.WriteLine("{0} has connected.", message.SenderConnection.Peer.Configuration.LocalAddress);
                        }
                        else if (_clients.Count >= 2)
                        {
                            Console.WriteLine("Too many clients, {0} rejected", message.SenderConnection.Peer.Configuration.LocalAddress);
                        }
                        if (message.SenderConnection.Status == NetConnectionStatus.Disconnected)
                        {
                            _clients.Remove(message.SenderConnection.Peer);
                            Console.WriteLine("{0} has disconnected.", message.SenderConnection.Peer.Configuration.LocalAddress);
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled message type: {message.MessageType}");
                        break;
                }
                _server.Recycle(message);
            }
        }
    }
}