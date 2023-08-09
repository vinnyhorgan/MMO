using System.Numerics;
using Raylib_cs;

namespace Client.Screens
{
    class GameScreen : Screen
    {
        public override void Load()
        {
            NetworkManager.Instance.Connect("127.0.0.1");
        }

        public override void Update(float dt)
        {

        }

        public override void Draw()
        {
            Raylib.DrawTextEx(Game.Instance.Font, "MMO", new Vector2(100, 100), Game.Instance.Font.baseSize, 0, Color.WHITE);

            foreach (var entity in NetworkManager.Instance._entities.Values)
            {
                Raylib.DrawCircleV(entity.Position, 10, Color.RED);
            }
        }
    }
}
