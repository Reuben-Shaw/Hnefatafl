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
    public enum SideType { Attackers, Defenders }

    sealed class Player
    {
        public enum InstructType { SELECT, MOVE, MOVEFAIL, WIN, LOSE, START, RESPONSE, GAMEOPTIONS, FULLPIECES }
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
                
                if (_side == SideType.Attackers) _board._turnDisplay._defendersTurn = !m_currentTurn;
                else _board._turnDisplay._defendersTurn = m_currentTurn;

                if (!_board.MovesStillPossible(_side))
                {
                    SendMessage(WIN.ToString());
                    Console.WriteLine("Lost due to no moves");
                }
            }
        }
        public SideType? _side; //Nullable as the when connecting to a server the player side will not be immediatly sent, and thus null, checks will be done before this however and allowing it to trap for null values prevents exceptions
        public List<string> _repeatedMoveChk = new List<string>();

        public bool _awaitingResponse;
        public double _timeSinceSend;
        public string _messageSent;

        public Player(GraphicsDeviceManager graphics, ContentManager Content,  UserOptions options)
        {
            _board = new Board(graphics, Content, options._boardColours, new Color[] {options._pawnAttacker, options._pawnDefender, options._pawnDefender}, options._selectColours);
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

        public void SendPieces()
        {
            Console.WriteLine("Attempting piece send");
            NetOutgoingMessage outMsg = _client.CreateMessage();

            Hashtable fullTable = _board.GetPieces().AllPieces();
            List<Piece> piecesToSend = new List<Piece>();

            foreach (DictionaryEntry entry in fullTable)
            {
                Piece piece = entry.Value as Piece; //Converts the DictionaryEntry into a Piece, as it will always be a Piece
                piecesToSend.Add(piece);
            }

            XmlSerializer xsSubmit = new XmlSerializer(typeof(List<Piece>));

            using(StringWriter writerS = new StringWriter())
            {
                using(XmlWriter writerX = XmlWriter.Create(writerS))
                {
                    xsSubmit.Serialize(writerX, piecesToSend);
                    outMsg.Write(FULLPIECES.ToString() + "," + writerS.ToString() + "," + _board.GetSelectPiece().ToString());
                }
            }

            _client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
        }

        public string CheckMessage()
        {
            NetIncomingMessage message = _client.ReadMessage();
        
            if (message is not null)
            {
                if (message.SenderConnection is not null && message.SenderConnection.RemoteUniqueIdentifier != _client.UniqueIdentifier)
                {
                    string msg = message.ReadString();
                    Console.WriteLine(msg);
                    string[] msgDiv = msg.Split(",");

                    if (msgDiv[0] == RESPONSE.ToString())
                    {
                        _awaitingResponse = false;
                        _timeSinceSend = 0;
                        return msg;
                    }
                    else if (msgDiv[0] == SELECT.ToString())
                    {
                        _board.SelectPiece(new HPoint(msgDiv[1], msgDiv[2]));
                        SendMessage(RESPONSE.ToString());
                    }
                    else if (msgDiv[0] == MOVE.ToString())
                    {
                        _board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                        _currentTurn = !_currentTurn;
                        SendMessage(RESPONSE.ToString());
                    }
                    else if (msgDiv[0] == MOVEFAIL.ToString())
                    {
                        _board.SelectPiece(new HPoint(-1, -1));
                        SendMessage(RESPONSE.ToString());
                    }
                    else if (msgDiv[0] == WIN.ToString())
                    {
                        Console.WriteLine("I won");
                        //_board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                    }
                    else if (msgDiv[0] == LOSE.ToString())
                    {
                        Console.WriteLine("I lost");
                        //_board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                    }
                    else if (msgDiv[0] == START.ToString())
                    {
                        _board._state = Board.BoardState.ActiveGame;
                    }
                    else if (msgDiv[0] == GAMEOPTIONS.ToString())
                    {
                        _board.CreateBoard((BoardTypes)Enum.Parse(typeof(BoardTypes), msgDiv[2]));
                        _board._serverOp = OptionsXmlDeserialise(msgDiv[1]);
                        
                        if (_side is null && _board._serverOp._playerTurn == ServerOptions.PlayerTurn.Attacker)
                        {
                            _side = SideType.Attackers;
                            _currentTurn = true;
                        }
                        else if (_side is null)
                        {
                            _side = SideType.Defenders;
                            _currentTurn = false;
                        }

                        Console.WriteLine("Completed deserialisation of options");
                    }
                    else if (msgDiv[0] == FULLPIECES.ToString())
                    {
                        List<Piece> deserialisedPieces = PiecesXmlDeserialise(msg);
                        if (_board.CheckPawns(deserialisedPieces))
                        {
                            _board.ReceivePawns(deserialisedPieces);
                            _currentTurn = !_currentTurn;
                        }
                        _board.SelectPiece(HPointXmlDeserialise(msg));
                        SendMessage(RESPONSE.ToString());
                        
                        Console.WriteLine("Completed deserialisation of server");
                    }

                    return msg;
                }
            }
        
            return "";
        }
        
        private ServerOptions OptionsXmlDeserialise(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ServerOptions));
            using (TextReader reader = new StringReader(xml))
            {
                return (ServerOptions) serializer.Deserialize(reader);
            }
        }

        private List<Piece> PiecesXmlDeserialise(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Piece>));

            using (TextReader reader = new StringReader(xml.Split(',')[1]))
            {
                return (List<Piece>) serializer.Deserialize(reader);
            }
        }

        private HPoint HPointXmlDeserialise(string xml)
        {
            return new HPoint(xml.Split(',')[2], xml.Split(',')[3]);
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