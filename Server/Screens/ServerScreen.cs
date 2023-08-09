using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Server.Screens
{
    public enum ServerToCliendMessage : ushort
    {
        WorldUpdate = 1
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

        private static Dictionary<ushort, Player> _players = new();
        private static Dictionary<ushort, ushort> _lastProcessedInputs = new();

        private int _updateRate = 10;
        private float _timer = 0;

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

            if (_timer >= 1f / _updateRate)
            {
                Tick();
                _timer = 0;
            }
        }

        public override void Draw()
        {
            Raylib.DrawTextEx(Game.Instance.Font, $"Server running on port: {_server.Port}", new Vector2(10, 10), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Server max clients: {_server.MaxClientCount}", new Vector2(10, 30), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Connected clients: {_server.ClientCount}", new Vector2(10, 50), Game.Instance.Font.baseSize, 0, Color.WHITE);

            for (int i = 0; i < _server.ClientCount; i++)
            {
                Raylib.DrawTextEx(Game.Instance.Font, $"Client {_server.Clients[i].Id}", new Vector2(10, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.WHITE);

                if (_server.Clients[i].IsConnected)
                {
                    Raylib.DrawTextEx(Game.Instance.Font, "Connected", new Vector2(200, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.GREEN);
                }
                else
                {
                    Raylib.DrawTextEx(Game.Instance.Font, "Disconnected", new Vector2(200, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.RED);
                }

                Raylib.DrawTextEx(Game.Instance.Font, $"Ping: {_server.Clients[i].SmoothRTT}", new Vector2(400, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.WHITE);
            }
        }

        public override void Unload()
        {
            _server.Stop();
        }

        [MessageHandler((ushort)ClientToServerMessage.PlayerInput)]
        private static void PlayerInput(ushort clientId, Message message)
        {
            var player = _players[clientId];

            var inputs = message.GetBools(4);

            player.Move(inputs);

            _lastProcessedInputs[clientId] = message.GetUShort();
        }

        private void PlayerConnected(object sender, ServerConnectedEventArgs e)
        {
            var player = new Player(e.Client.Id, new Vector2(Raylib.GetRandomValue(0, 1280), Raylib.GetRandomValue(0, 720)));
            _players.Add(e.Client.Id, player);
        }

        private void PlayerDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            _players.Remove(e.Client.Id);
        }

        private void Tick()
        {
            var message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToCliendMessage.WorldUpdate);

            message.AddUShort((ushort)_players.Count);

            foreach (var player in _players.Values)
            {
                message.AddUShort(player.Id);
                message.AddInt((int)player.Position.X);
                message.AddInt((int)player.Position.Y);

                if (_lastProcessedInputs.ContainsKey(player.Id))
                {
                    message.AddUShort(_lastProcessedInputs[player.Id]);
                }
                else
                {
                    message.AddUShort(0);
                }
            }

            _server.SendToAll(message);
        }
    }
}
