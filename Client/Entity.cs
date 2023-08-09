using System;
using System.Collections.Generic;
using System.Numerics;

namespace Client
{
    class Entity
    {
        public ushort Id;
        public Vector2 Position;
        public int Speed = 5;
        public List<NetworkManager.Position> Positions = new();

        public Entity(ushort id, Vector2 position)
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

        public void AddPosition(Vector2 position)
        {
            Positions.Add(new NetworkManager.Position(DateTimeOffset.Now.ToUnixTimeMilliseconds(), position));
        }
    }
}
