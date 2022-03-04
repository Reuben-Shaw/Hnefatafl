using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Lidgren.Network;

using static Hnefatafl.Player.InstructType;


namespace Hnefatafl
{
    sealed class Player
    {
        public enum InstructType { SELECT, MOVE, MOVEFAIL, WIN }
        public enum SideType { Attackers, Defenders }
        public Board _board;
        private NetClient _client;
        public bool _currentTurn;
        public SideType? _side;

        public Player(GraphicsDeviceManager graphics, ContentManager Content, int boardSize)
        {
            _board = new Board(graphics, Content, boardSize);
            CreateClient();
        }

        private NetPeerConfiguration CreateConfig()
        {
            return new NetPeerConfiguration("Hnefatafl");
        }

        private void CreateClient()
        {
            _client = new NetClient(CreateConfig());
            Console.WriteLine(_client.ConnectionsCount);
            _client.Start();
        }

        public bool IsConnected()
        {
            if (_client.ConnectionStatus == NetConnectionStatus.Connected)
                return true;
            else
                return false;
        }

        public bool SendMessage(string message)
        {
            if (IsConnected())
            {
                _client.SendMessage(_client.CreateMessage(message), NetDeliveryMethod.ReliableOrdered);
                _client.FlushSendQueue();
                return true;
            }
          

            return false;
        }

        public void SendOptions()
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(ServerOptions));
            NetOutgoingMessage outMsg = _client.CreateMessage();

            using(StringWriter writerS = new StringWriter())
            {
                using(XmlWriter writerX = XmlWriter.Create(writerS))
                {
                    xsSubmit.Serialize(writerX, _board._serverOp);
                    outMsg.Write(writerS.ToString());
                }
            }
            _client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
        }

        public void CheckMessage()
        {
            NetIncomingMessage message = _client.ReadMessage();
            try
            {
                if (message is not null)
                {
                    if (message.SenderConnection != null && message.SenderConnection.RemoteUniqueIdentifier != _client.UniqueIdentifier)
                    {
                        string msg = message.ReadString();
                        Console.WriteLine(msg);
                        string[] msgDiv = msg.Split(",");
                        
                        if (msgDiv[0] == SELECT.ToString())
                        {
                            _board.SelectPiece(new HPoint(msgDiv[1], msgDiv[2]));
                        }
                        else if (msgDiv[0] == MOVE.ToString())
                        {
                            _currentTurn = !_currentTurn;
                            Console.WriteLine("Is it my turn? " + _currentTurn);
                            _board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                        }
                        else if (msgDiv[0] == MOVEFAIL.ToString())
                        {
                            _board.SelectPiece(new HPoint(-1, -1));
                        }
                        else
                        {
                            _board._serverOp = OptionsXmlDeserialise(msg);
                            
                            if (_side is null && _board._serverOp._playerTurn == ServerOptions.PlayerTurn.Attacker)
                            {
                                _currentTurn = true;
                                _side = Player.SideType.Attackers;
                            }
                            else if (_side is null)
                            {
                                _currentTurn = false;
                                _side = Player.SideType.Defenders;
                            }

                            Console.WriteLine("Completed deserialisation of options");
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Poor message formatting - probably startup message. Crash averted");
            }
        }

        private ServerOptions OptionsXmlDeserialise(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ServerOptions));

            using (TextReader reader = new StringReader(xml))
            {
                return (ServerOptions) serializer.Deserialize(reader);
            }
        }

        public void EstablishConnection(string ip, int port)
        {
            try
            {
                _client.Connect(ip, port);
            }
            catch
            {
                Console.WriteLine("Failed to connect");
            }
        }

        public void Disconnect()
        {
            _client.Disconnect("Manual Disconnect");
        }
    }
}