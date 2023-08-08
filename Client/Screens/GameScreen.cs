using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Client.Screens
{
    public enum ServerToCliendMessage : ushort
    {
        SpawnPlayer = 1,
        PlayerMovement
    }

    public enum ClientToServerMessage : ushort
    {
        PlayerInput = 1
    }

    class GameScreen : Screen
    {
        private Riptide.Client _client;
        private static Dictionary<ushort, Player> _players = new();
        private Player _localPlayer;
        private bool[] _inputs = new bool[4];

        public override void Load()
        {
            _client = new Riptide.Client();
            _client.Connected += Connected;
            _client.Disconnected += Disconnected;
            _client.ConnectionFailed += ConnectionFailed;
            _client.ClientDisconnected += PlayerLeft;

            _client.Connect("127.0.0.1:7777");
        }

        public override void Update(float dt)
        {
            _client.Update();

            // terribile...
            if (_localPlayer == null)
            {
                foreach (var player in _players.Values)
                {
                    if (player.Id == _client.Id)
                    {
                        _localPlayer = player;
                        break;
                    }
                }
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                _inputs[0] = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                _inputs[1] = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                _inputs[2] = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                _inputs[3] = true;
            }

            var message = Message.Create(MessageSendMode.Unreliable, (ushort)ClientToServerMessage.PlayerInput);
            message.AddBools(_inputs, false);
            _client.Send(message);

            _inputs[0] = false;
            _inputs[1] = false;
            _inputs[2] = false;
            _inputs[3] = false;
        }

        public override void Draw()
        {
            foreach (var player in _players.Values)
            {
                Raylib.DrawCircleV(player.Position, 10, Color.RED);
            }
        }

        [MessageHandler((ushort)ServerToCliendMessage.SpawnPlayer)]
        private static void SpawnPlayer(Message message)
        {
            var player = new Player(message.GetUShort(), new Vector2(message.GetInt(), message.GetInt()));
            _players.Add(player.Id, player);

            Logger.Info($"Player spawned with ID: {player.Id}");
        }

        [MessageHandler((ushort)ServerToCliendMessage.PlayerMovement)]
        private static void PlayerMovement(Message message)
        {
            var id = message.GetUShort();

            if (!_players.ContainsKey(id))
                return;

            var player = _players[id];

            player.Position = new Vector2(message.GetInt(), message.GetInt());
        }

        private void Connected(object sender, EventArgs e)
        {
            Logger.Info("Connected to server!");
        }

        private void Disconnected(object sender, DisconnectedEventArgs e)
        {
            _players.Clear();

            Logger.Info("Disconnected from server!");
        }

        private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Logger.Error("Failed to connect to server!");
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            _players.Remove(e.Id);

            Logger.Info($"Player {e.Id} left the server!");
        }
    }
}
