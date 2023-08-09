using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Client.Screens
{
    public enum ServerToCliendMessage : ushort
    {
        WorldUpdate = 1
    }

    public enum ClientToServerMessage : ushort
    {
        PlayerInput = 1
    }

    class Input
    {
        public bool[] Inputs;
        public ushort InputSequenceNumber;

        public Input(bool[] inputs, ushort inputSequenceNumber)
        {
            Inputs = inputs;
            InputSequenceNumber = inputSequenceNumber;
        }
    }

    class GameScreen : Screen
    {
        private Riptide.Client _client;

        private static Dictionary<ushort, Player> _players = new();
        private static List<Input> _pendingInputs = new();
        private static ushort _inputSequenceNumber = 0;
        private static bool _serverReconciliation = true;
        private static bool _playerInterpolation = true;
        private static ushort _localId = 0;

        private bool[] _inputs = new bool[4];
        private bool _clientSidePrediction = true;
        private int _updateRate = 50;
        private float _timer = 0;

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
            _timer += dt;

            _client.Update();

            if (_timer >= 1f / _updateRate)
            {
                Tick();
                _timer = 0;
            }
        }

        public override void Draw()
        {
            foreach (var player in _players.Values)
            {
                Raylib.DrawCircleV(player.Position, 10, Color.RED);
            }
        }

        [MessageHandler((ushort)ServerToCliendMessage.WorldUpdate)]
        private static void WorldUpdate(Message message)
        {
            ushort playerCount = message.GetUShort();

            for (int i = 0; i < playerCount; i++)
            {
                ushort id = message.GetUShort();
                int x = message.GetInt();
                int y = message.GetInt();
                ushort lastProcessedInput = message.GetUShort();

                if (!_players.ContainsKey(id))
                {
                    _players.Add(id, new Player(id, new Vector2(x, y)));
                }

                var player = _players[id];

                if (player.Id == _localId)
                {
                    player.Position = new Vector2(x, y);

                    if (_serverReconciliation)
                    {
                        int j = 0;

                        while (j < _pendingInputs.Count)
                        {
                            var input = _pendingInputs[j];

                            if (input.InputSequenceNumber <= lastProcessedInput)
                            {
                                _pendingInputs.RemoveAt(j);
                            }
                            else
                            {
                                player.Move(input.Inputs);
                                j++;
                            }
                        }
                    }
                    else
                    {
                        _pendingInputs.Clear();
                    }
                }
                else
                {
                    if (!_playerInterpolation)
                    {
                        player.Position = new Vector2(x, y);
                    }
                    else
                    {
                        player.AddPosition(new Vector2(x, y));
                    }
                }
            }
        }

        private void Connected(object sender, EventArgs e)
        {
            Logger.Info("Connected to server!");

            _localId = _client.Id;
        }

        private void Disconnected(object sender, DisconnectedEventArgs e)
        {
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

        private void Tick()
        {
            if (_localId == 0)
            {
                return;
            }

            var moved = false;

            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                _inputs[0] = true;

                moved = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                _inputs[1] = true;

                moved = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                _inputs[2] = true;

                moved = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                _inputs[3] = true;

                moved = true;
            }

            if (moved)
            {
                var message = Message.Create(MessageSendMode.Unreliable, (ushort)ClientToServerMessage.PlayerInput);
                message.AddBools(_inputs, false);
                message.AddUShort(_inputSequenceNumber++);

                _client.Send(message);

                if (_clientSidePrediction)
                {
                    if (_players.ContainsKey(_localId))
                    {
                        _players[_localId].Move(_inputs);
                    }
                }

                bool[] copy = new bool[4];
                Array.Copy(_inputs, copy, 4);

                _pendingInputs.Add(new Input(copy, (ushort)(_inputSequenceNumber - 1)));

                _inputs = new bool[4];
            }

            if (_playerInterpolation)
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long renderTimestamp = now - (1000 / 10); // 10 is the server tick rate

                foreach (var player in _players.Values)
                {
                    if (player.Id == _localId)
                    {
                        continue;
                    }

                    List<Position> buffer = player.Positions;

                    // Drop older positions.
                    while (buffer.Count >= 2 && buffer[1].Timestamp <= renderTimestamp)
                    {
                        buffer.RemoveAt(0);
                    }

                    // Interpolate between the two surrounding authoritative positions.
                    if (buffer.Count >= 2 && buffer[0].Timestamp <= renderTimestamp && renderTimestamp <= buffer[1].Timestamp)
                    {
                        Vector2 pos0 = buffer[0].Value;
                        Vector2 pos1 = buffer[1].Value;
                        long t0 = buffer[0].Timestamp;
                        long t1 = buffer[1].Timestamp;

                        float interpolationFactor = (renderTimestamp - t0) / (float)(t1 - t0);

                        float interpolatedX = pos0.X + (pos1.X - pos0.X) * interpolationFactor;
                        float interpolatedY = pos0.Y + (pos1.Y - pos0.Y) * interpolationFactor;

                        player.Position = new Vector2(interpolatedX, interpolatedY);
                    }
                }
            }
        }
    }
}
