using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Server.Screens
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

    class ServerScreen : Screen
    {
        private int _port = 7777;
        private int _maxPlayers = 10;

        private Riptide.Server _server;
        private int _bytesReceived = 0;
        private int _bytesReceivedPerSecond = 0;
        private float _timer = 0;

        private static Dictionary<ushort, Player> _players = new();

        public override void Load()
        {
            _server = new Riptide.Server();
            _server.ClientConnected += PlayerConnected;
            _server.ClientDisconnected += PlayerDisconnected;

            _server.Start((ushort)_port, (ushort)_maxPlayers);
        }

        public override void Update(float dt)
        {
            _timer += dt;

            _server.Update();

            if (_timer >= 1)
            {
                _bytesReceivedPerSecond = _bytesReceived;
                _bytesReceived = 0;
                _timer = 0;
            }

            foreach (var player in _players.Values)
            {
                if (player.Inputs[0])
                {
                    player.Position += new Vector2(0, -1);
                }

                if (player.Inputs[1])
                {
                    player.Position += new Vector2(0, 1);
                }

                if (player.Inputs[2])
                {
                    player.Position += new Vector2(-1, 0);
                }

                if (player.Inputs[3])
                {
                    player.Position += new Vector2(1, 0);
                }

                var message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToCliendMessage.PlayerMovement);
                message.AddUShort(player.Id);
                message.AddInt((int)player.Position.X);
                message.AddInt((int)player.Position.Y);

                _server.SendToAll(message);
            }
        }

        public override void Draw()
        {
            Raylib.DrawTextEx(Game.Instance.Font, $"Server running on port: {_server.Port}", new Vector2(10, 10), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Server max clients: {_server.MaxClientCount}", new Vector2(10, 30), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Connected clients: {_server.ClientCount}", new Vector2(10, 50), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Bytes received/s: {_bytesReceivedPerSecond}", new Vector2(10, 70), Game.Instance.Font.baseSize, 0, Color.WHITE);

            for (int i = 0; i < _server.ClientCount; i++)
            {
                Raylib.DrawTextEx(Game.Instance.Font, $"Client {_server.Clients[i].Id}", new Vector2(10, 120 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.WHITE);

                if (_server.Clients[i].IsConnected)
                {
                    Raylib.DrawTextEx(Game.Instance.Font, "Connected", new Vector2(200, 120 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.GREEN);
                }
                else
                {
                    Raylib.DrawTextEx(Game.Instance.Font, "Disconnected", new Vector2(200, 120 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.RED);
                }

                Raylib.DrawTextEx(Game.Instance.Font, $"Ping: {_server.Clients[i].SmoothRTT}", new Vector2(400, 120 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.WHITE);
            }
        }

        public override void Unload()
        {
            _server.Stop();
        }

        [MessageHandler((ushort)ClientToServerMessage.PlayerInput)]
        private static void PlayerInput(ushort clientId, Message message)
        {
            if (!_players.ContainsKey(clientId))
                return;

            Player player = _players[clientId];
            message.GetBools(4, player.Inputs);
        }

        private void PlayerConnected(object sender, ServerConnectedEventArgs e)
        {
            Message message;

            foreach (var player in _players.Values)
            {
                message = Message.Create(MessageSendMode.Reliable, ServerToCliendMessage.SpawnPlayer);
                message.AddUShort(player.Id);
                message.AddInt((int)player.Position.X);
                message.AddInt((int)player.Position.Y);

                _server.Send(message, e.Client.Id);
            }

            int spawnX = Raylib.GetRandomValue(0, 1280);
            int spawnY = Raylib.GetRandomValue(0, 720);

            message = Message.Create(MessageSendMode.Reliable, ServerToCliendMessage.SpawnPlayer);
            message.AddUShort(e.Client.Id);
            message.AddInt(spawnX); // Spawn position X
            message.AddInt(spawnY); // Spawn position Y

            _server.SendToAll(message);

            _players.Add(e.Client.Id, new Player(e.Client.Id, new Vector2(spawnX, spawnY)));
        }

        private void PlayerDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            _players.Remove(e.Client.Id);
        }
    }
}
