using System;
using System.Collections.Generic;
using System.Numerics;

namespace Client
{
    class Position
    {
        public long Timestamp;
        public Vector2 Value;

        public Position(long timestamp, Vector2 value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }

    class Player
    {
        public ushort Id;
        public Vector2 Position;
        public int Speed = 5;
        public List<Position> Positions = new();

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

        public void AddPosition(Vector2 position)
        {
            Positions.Add(new Position(DateTimeOffset.Now.ToUnixTimeMilliseconds(), position));
        }
    }
}
