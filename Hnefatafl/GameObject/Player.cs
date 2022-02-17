using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;


namespace Hnefatafl
{
    public sealed class Player
    {
        public Board _board;
        private bool _connected;
        private NetClient _client;

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
                    if (message.ReadString() == "hey")
                    {
                        Hnefatafl.testBool = true;
                    }
                }
            }
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