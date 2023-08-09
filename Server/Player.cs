using System.Numerics;

namespace Server
{
    class Player : IEntity
    {
        public int Speed = 5;

        private EntityTypes _type = EntityTypes.Player;
        private ushort _id;
        private Vector2 _position;

        public ushort Id
        {
            get { return _id; }
        }

        public EntityTypes Type
        {
            get { return _type; }
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public Player(ushort id, Vector2 position)
        {
            _id = id;
            _position = position;
        }

        public void Update()
        {

        }

        public void Move(bool[] input)
        {
            if (input[0])
            {
                _position.Y -= Speed;
            }

            if (input[1])
            {
                _position.Y += Speed;
            }

            if (input[2])
            {
                _position.X -= Speed;
            }

            if (input[3])
            {
                _position.X += Speed;
            }
        }
    }
}
