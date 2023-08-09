using System.Numerics;

namespace Server
{
    class Player
    {
        public ushort Id;
        public Vector2 Position;
        public int Speed = 5;

        public Player(ushort id, Vector2 position)
        {
            Id = id;
            Position = position;
        }

        public void Move(bool[] input)
        {
            if (input[0])
            {
                Position.Y -= Speed;
            }

            if (input[1])
            {
                Position.Y += Speed;
            }

            if (input[2])
            {
                Position.X -= Speed;
            }

            if (input[3])
            {
                Position.X += Speed;
            }
        }
    }
}
