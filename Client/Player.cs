using System.Numerics;

namespace Client
{
    class Player
    {
        private ushort _id;
        private Vector2 _position;

        public ushort Id
        {
            get { return _id; }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Player(ushort id, Vector2 position)
        {
            _id = id;
            _position = position;
        }
    }
}
