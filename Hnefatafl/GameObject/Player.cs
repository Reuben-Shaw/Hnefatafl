using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;

using static Hnefatafl.Player.InstructType;


namespace Hnefatafl
{
    sealed class Player
    {
        public enum InstructType { SELECT, MOVE, MOVEFAIL }
        public enum SideType { Attackers, Defenders }
        public Board _board;
        private bool _connected;
        private NetClient _client;
        public bool _currentTurn = true;
        public SideType _side = SideType.Attackers;

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

        public bool SendMessage(string message)
        {
            if (_connected)
            {
                _client.SendMessage(_client.CreateMessage(message), NetDeliveryMethod.ReliableOrdered);
                _client.FlushSendQueue();
                return true;
            }
            return false;
        }

        public void CheckMessage()
        {
            if (_connected)
            {
                NetIncomingMessage message;

                if ((message = _client.ReadMessage()) != null)
                {
                    if (message.SenderConnection.RemoteUniqueIdentifier != _client.UniqueIdentifier)
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
                            _board.MakeMove(new HPoint(msgDiv[1], msgDiv[2]), _side, true);
                        }
                        else if (msgDiv[0] == MOVEFAIL.ToString())
                        {
                            _board.SelectPiece(new HPoint(-1, -1));
                        }
                        else if (IsBool(msgDiv[0]))
                        {
                            _currentTurn = Convert.ToBoolean(msgDiv[0]);
                            if (_currentTurn == false)
                                _side = SideType.Defenders;
                        }
                    }
                }
            }
        }

        public bool IsBool(string boolChk)
        {
            if (boolChk.ToLower() == "true")
                return true;
            else if (boolChk.ToLower() == "false")
                return true;
            return false;
        }

        public void EstablishConnection(string ip, int port)
        {
            _client.Connect(ip, port);
            _connected = true;
        }

        public void EstablishConnection(string ip)
        {
            _client.Connect(ip, 14242);
            _connected = true;
        }

        public void EstablishConnection()
        {
            _client.Connect("localhost", 14242);
            _connected = true;
        }
    }
}