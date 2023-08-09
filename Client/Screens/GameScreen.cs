using System.Numerics;
using Raylib_cs;

namespace Client.Screens
{
    class GameScreen : Screen
    {
        private Camera2D _camera;

        public override void Load()
        {
            NetworkManager.Instance.Connect("127.0.0.1");

            _camera = new Camera2D
            {
                target = Vector2.Zero,
                offset = new Vector2(Raylib.GetScreenWidth() / 2.0f, Raylib.GetScreenHeight() / 2.0f),
                rotation = 0.0f,
                zoom = 1.0f
            };
        }

        public override void Update(float dt)
        {
            if (NetworkManager.Instance.Entities.ContainsKey(NetworkManager.Instance.Id))
            {
                _camera.target = NetworkManager.Instance.Entities[NetworkManager.Instance.Id].Position;
            }
        }

        public override void Draw()
        {
            Raylib.DrawTextEx(Game.Instance.Font, "MMO", new Vector2(100, 100), Game.Instance.Font.baseSize, 0, Color.WHITE);

            Raylib.BeginMode2D(_camera);

            foreach (var entity in NetworkManager.Instance.Entities.Values)
            {
                if (entity.Type == EntityTypes.Player)
                {
                    if (entity.Id == NetworkManager.Instance.Id)
                    {
                        Raylib.DrawCircleV(entity.Position, 10, Color.BLUE);
                    }
                    else
                    {
                        Raylib.DrawCircleV(entity.Position, 10, Color.RED);
                    }
                }
                else if (entity.Type == EntityTypes.Creature)
                {
                    Raylib.DrawCircleV(entity.Position, 10, Color.ORANGE);
                }
            }

            Raylib.EndMode2D();
        }
    }
}
