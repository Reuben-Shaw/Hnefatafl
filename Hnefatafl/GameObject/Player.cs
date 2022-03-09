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
        public enum InstructType { SELECT, MOVE, MOVEFAIL, WIN, LOSE, START }
        public enum SideType { Attackers, Defenders }
        public Board _board;
        private NetClient _client;
        private bool m_currentTurn;
        public bool _currentTurn
        {
            get
            {
                return m_currentTurn;
            }
            set
            {
                m_currentTurn = value;
                if (!_board.MovesStillPossible(_side))
                {
                    SendMessage(WIN.ToString());
                    Console.WriteLine("Lost due to no moves");
                }
            }
        }
        public SideType? _side; //Nullable as the when connecting to a server the player side will not be immediatly sent, and thus null, checks will be done before this however and allowing it to trap for null values prevents exceptions
        public List<string> _repeatedMoveChk = new List<string>();

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
                    if (message.SenderConnection is not null && message.SenderConnection.RemoteUniqueIdentifier != _client.UniqueIdentifier)
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
                            _board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                            _currentTurn = !_currentTurn;
                        }
                        else if (msgDiv[0] == MOVEFAIL.ToString())
                        {
                            _board.SelectPiece(new HPoint(-1, -1));
                        }
                        else if (msgDiv[0] == WIN.ToString())
                        {
                            Console.WriteLine("I won");
                            _board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                        }
                        else if (msgDiv[0] == LOSE.ToString())
                        {
                            Console.WriteLine("I lost");
                            _board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                        }
                        else if (msgDiv[0] == START.ToString())
                        {
                            _board._state = Board.BoardState.ActiveGame;
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