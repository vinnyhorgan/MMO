using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Client
{
    enum EntityTypes : byte
    {
        Player = 1,
        Creature
    }

    enum ServerToCliendMessage : ushort
    {
        WorldUpdate = 1
    }

    enum ClientToServerMessage : ushort
    {
        PlayerInput = 1
    }

    public class Input
    {
        public bool[] Inputs;
        public ushort InputSequenceNumber;

        public Input(bool[] inputs, ushort inputSequenceNumber)
        {
            Inputs = inputs;
            InputSequenceNumber = inputSequenceNumber;
        }
    }

    public class Position
    {
        public long Timestamp;
        public Vector2 Value;

        public Position(long timestamp, Vector2 value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }

    class NetworkManager
    {
        private static NetworkManager _instance;

        private Riptide.Client _client;
        private int _updateRate = 50;
        private float _timer = 0;
        private ushort _id = 0;
        private Dictionary<ushort, Entity> _entities = new();
        private bool[] _inputs = new bool[4];
        private List<Input> _pendingInputs = new();
        private ushort _inputSequenceNumber = 0;

        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetworkManager();
                }

                return _instance;
            }
        }

        public ushort Id
        {
            get { return _id; }
        }

        public Dictionary<ushort, Entity> Entities
        {
            get { return _entities; }
        }

        public void Connect(string ip, int port = 7777)
        {
            _client = new Riptide.Client();
            _client.Connected += Connected;
            _client.Disconnected += Disconnected;
            _client.ConnectionFailed += ConnectionFailed;
            _client.ClientDisconnected += PlayerLeft;

            _client.Connect($"{ip}:{port}");
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Update(float dt)
        {
            if (_client != null)
            {
                _client.Update();

                _timer += dt;

                if (_timer >= 1f / _updateRate)
                {
                    Tick();

                    _timer = 0;
                }
            }
        }

        [MessageHandler((ushort)ServerToCliendMessage.WorldUpdate)]
        private static void WorldUpdate(Message message)
        {
            ushort entityCount = message.GetUShort();

            for (int i = 0; i < entityCount; i++)
            {
                ushort id = message.GetUShort();
                EntityTypes type = (EntityTypes)message.GetByte();
                int x = message.GetInt();
                int y = message.GetInt();
                ushort lastProcessedInput = message.GetUShort();

                if (!Instance._entities.ContainsKey(id))
                {
                    Instance._entities.Add(id, new Entity(id, type, new Vector2(x, y)));
                }

                var entity = Instance._entities[id];

                if (entity.Id == Instance._id)
                {
                    entity.Position = new Vector2(x, y);

                    // Server reconciliation
                    int j = 0;

                    while (j < Instance._pendingInputs.Count)
                    {
                        var input = Instance._pendingInputs[j];

                        if (input.InputSequenceNumber <= lastProcessedInput)
                        {
                            Instance._pendingInputs.RemoveAt(j);
                        }
                        else
                        {
                            entity.Move(input.Inputs);
                            j++;
                        }
                    }
                }
                else
                {
                    // Entity interpolation
                    entity.AddPosition(new Vector2(x, y));
                }
            }
        }

        private void Tick()
        {
            if (_id == 0)
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

                // Client-side prediction
                if (_entities.ContainsKey(_id))
                {
                    _entities[_id].Move(_inputs);
                }

                bool[] copy = new bool[4];
                Array.Copy(_inputs, copy, 4);

                _pendingInputs.Add(new Input(copy, (ushort)(_inputSequenceNumber - 1)));

                _inputs = new bool[4];
            }

            // Entity interpolation
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long renderTimestamp = now - (1000 / 10); // 10 is the server tick rate

            foreach (var entity in _entities.Values)
            {
                if (entity.Id == _id)
                {
                    continue;
                }

                List<Position> buffer = entity.Positions;

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

                    entity.Position = new Vector2(interpolatedX, interpolatedY);
                }
            }
        }

        private void Connected(object sender, EventArgs e)
        {
            _id = _client.Id;
        }

        private void Disconnected(object sender, DisconnectedEventArgs e)
        {
        }

        private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            _entities.Remove(e.Id);
        }
    }
}
