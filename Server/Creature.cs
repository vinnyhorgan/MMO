using System;
using System.Numerics;
using Raylib_cs;

namespace Server
{
    class Creature : IEntity
    {
        public int Speed = 5;
        public bool Mutated = false;

        private EntityTypes _type = EntityTypes.Creature;
        private ushort _id;
        private Vector2 _position;

        private NN _nn;

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

        public Creature(ushort id, Vector2 position)
        {
            _id = id;
            _position = position;
            _nn = new NN(new int[] { 2, 32, 2 });
        }

        public void Update()
        {
            if (!Mutated)
            {
                float amount = Raylib.GetRandomValue(0, 100) / 100.0f;
                float chance = Raylib.GetRandomValue(0, 100) / 100.0f;

                amount = MathF.Max(amount, 0);
                chance = MathF.Max(chance, 0);

                _nn.MutateNetwork(amount, chance);
                Mutated = true;
            }

            // random inputs
            float[] inputsToNN = new float[] { Raylib.GetRandomValue(-100, 100) / 100f, Raylib.GetRandomValue(-100, 100) / 100f };

            float[] outputsFromNN = _nn.Brain(inputsToNN);

            _position.X += outputsFromNN[0] * Speed;
            _position.Y += outputsFromNN[1] * Speed;
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
