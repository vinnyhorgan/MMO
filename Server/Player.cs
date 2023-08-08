using System.Numerics;

namespace Server
{
    class Player
    {
        private ushort _id;
        private Vector2 _position;
        private bool[] _inputs = new bool[4];

        public ushort Id
        {
            get { return _id; }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public bool[] Inputs
        {
            get { return _inputs; }
            set { _inputs = value; }
        }

        public Player(ushort id, Vector2 position)
        {
            _id = id;
            _position = position;
        }
    }
}
