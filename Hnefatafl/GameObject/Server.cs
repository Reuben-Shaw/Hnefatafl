using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Hnefatafl
{
    sealed class Server
    {
        private NetServer _server;
        private List<NetPeer> _clients;
        ServerOptions _serverOp;

        public void StartServer(int port, ServerOptions serverOp)
        {
            _serverOp = serverOp;
            NetPeerConfiguration config = new NetPeerConfiguration("Hnefatafl") { Port = port };
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
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
            _server.Shutdown("Server closed");
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
                        //Recieved messages
                        {
                            string data = message.ReadString();

                            if (_server.Connections[0] == message.SenderConnection && _clients.Count == 2)
                            {
                                _server.SendMessage(_server.CreateMessage(data), _server.Connections[1], NetDeliveryMethod.ReliableOrdered, 0);
                            }
                            else if (_clients.Count == 2)
                            {
                                _server.SendMessage(_server.CreateMessage(data), _server.Connections[0], NetDeliveryMethod.ReliableOrdered, 0);
                            }
                            break;
                        }
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine(message.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        //Connections
                        Console.WriteLine(message.SenderConnection.Status);
                        if (message.SenderConnection.Status == NetConnectionStatus.Connected && _clients.Count < 2)
                        {
                            _clients.Add(message.SenderConnection.Peer);

                            if (_clients.Count == 2) //Responsible for sending the correct turn to the second player to connect
                            {
                                if (_serverOp._playerTurn == ServerOptions.PlayerTurn.Attacker) _serverOp._playerTurn = ServerOptions.PlayerTurn.Defender;
                                else _serverOp._playerTurn = ServerOptions.PlayerTurn.Attacker;
                            }
                            string serialMsg = OptionXmlSerialise(_serverOp);

                            NetOutgoingMessage outMsg = _server.CreateMessage();
                            outMsg.Write(serialMsg);
                            _server.SendMessage(outMsg, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);

                            Console.WriteLine("{0} has connected.", message.SenderConnection.Peer.Configuration.LocalAddress);

                            if (_clients.Count == 2) //Responsible for sending the message to clients that the game is to start
                            {
                                _server.SendMessage(_server.CreateMessage(Player.InstructType.START.ToString()), _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                            }
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

        private string OptionXmlSerialise(ServerOptions options)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(ServerOptions));

            using(StringWriter writerS = new StringWriter())
            {
                using(XmlWriter writerX = XmlWriter.Create(writerS))
                {
                    xsSubmit.Serialize(writerX, options);
                    return writerS.ToString();
                }
            }
        }
    }
}