using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Server
{
    interface IEntity
    {
        public ushort Id { get; }
        public EntityTypes Type { get; }
        public Vector2 Position { get; }

        public void Move(bool[] input);
        public void Update();
    }

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

    class NetworkManager
    {
        public Dictionary<ushort, IEntity> Entities = new();

        private static NetworkManager _instance;

        private int _port = 7777;
        private int _maxPlayers = 10;

        private Riptide.Server _server;
        private int _updateRate = 10;
        private float _timer = 0;
        private Dictionary<ushort, ushort> _lastProcessedInputs = new();

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

        public Riptide.Server Server
        {
            get { return _server; }
        }

        public void Start()
        {
            _server = new Riptide.Server();
            _server.ClientConnected += PlayerConnected;
            _server.ClientDisconnected += PlayerDisconnected;

            _server.Start((ushort)_port, (ushort)_maxPlayers);
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void Update(float dt)
        {
            if (_server != null)
            {
                _server.Update();

                _timer += Raylib.GetFrameTime();

                if (_timer >= 1f / _updateRate)
                {
                    Tick();

                    _timer = 0;
                }
            }
        }

        [MessageHandler((ushort)ClientToServerMessage.PlayerInput)]
        private static void PlayerInput(ushort clientId, Message message)
        {
            var inputs = message.GetBools(4);

            var entity = Instance.Entities[clientId];

            entity.Move(inputs);

            Instance._lastProcessedInputs[clientId] = message.GetUShort();
        }

        private void Tick()
        {
            var message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToCliendMessage.WorldUpdate);

            message.AddUShort((ushort)Entities.Count);

            foreach (var entity in Entities.Values)
            {
                message.AddUShort(entity.Id);
                message.AddByte((byte)entity.Type);
                message.AddInt((int)entity.Position.X);
                message.AddInt((int)entity.Position.Y);

                if (_lastProcessedInputs.ContainsKey(entity.Id))
                {
                    message.AddUShort(_lastProcessedInputs[entity.Id]);
                }
                else
                {
                    message.AddUShort(0);
                }
            }

            _server.SendToAll(message);
        }

        private void PlayerConnected(object sender, ServerConnectedEventArgs e)
        {
            var entity = new Player(e.Client.Id, new Vector2(Raylib.GetRandomValue(0, 1280), Raylib.GetRandomValue(0, 720)));
            Entities.Add(e.Client.Id, entity);
        }

        private void PlayerDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            Entities.Remove(e.Client.Id);
        }
    }
}
