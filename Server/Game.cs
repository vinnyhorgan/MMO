using System;
using System.Numerics;
using Raylib_cs;
using Riptide.Utils;

namespace Server
{
    class Game
    {
        private static Game _instance;

        private bool _debug = true;
        private int _gameWidth = 800;
        private int _gameHeight = 600;
        private RenderTexture2D _target;
        private Font _font;
        private ImguiController _controller;
        private Vector2 _mouse;

        public static Game Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Game();
                }

                return _instance;
            }
        }

        public int Width
        {
            get { return _gameWidth; }
        }

        public int Height
        {
            get { return _gameHeight; }
        }

        public Font Font
        {
            get { return _font; }
        }

        public Vector2 Mouse
        {
            get { return _mouse; }
        }

        private Game()
        {
            Logger.Init(_debug);

            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
            Raylib.InitWindow(_gameWidth, _gameHeight, "Server");
            Raylib.SetWindowMinSize(_gameWidth / 2, _gameHeight / 2);
            Raylib.SetExitKey(KeyboardKey.KEY_NULL);
            Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor()));

            Raylib.InitAudioDevice();

            _target = Raylib.LoadRenderTexture(_gameWidth, _gameHeight);
            Raylib.SetTextureFilter(_target.texture, TextureFilter.TEXTURE_FILTER_POINT);

            Raylib.SetWindowIcon(Raylib.LoadImageFromTexture(AssetManager.Instance.LoadTexture("logo.png")));

            _font = AssetManager.Instance.LoadFont("roboto.ttf", 18);

            _controller = new ImguiController();
            _controller.Load(_gameWidth, _gameHeight);

            RiptideLogger.Initialize(Logger.Debug, Logger.Info, Logger.Warning, Logger.Error, false);

            ScreenManager.Instance.SetScreen(new Screens.ServerScreen());
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                float scale = MathF.Min(
                    (float)Raylib.GetScreenWidth() / _gameWidth,
                    (float)Raylib.GetScreenHeight() / _gameHeight
                );

                Vector2 mouse = Raylib.GetMousePosition();
                Vector2 virtualMouse = Vector2.Zero;
                virtualMouse.X = (mouse.X - (Raylib.GetScreenWidth() - (_gameWidth * scale)) * 0.5f) / scale;
                virtualMouse.Y = (mouse.Y - (Raylib.GetScreenHeight() - (_gameHeight * scale)) * 0.5f) / scale;

                var max = new Vector2(_gameWidth, _gameHeight);
                virtualMouse = Vector2.Clamp(virtualMouse, Vector2.Zero, max);

                _mouse = virtualMouse;

                float dt = Raylib.GetFrameTime();

                _controller.Update(dt);

                ScreenManager.Instance.Update(dt);

                Raylib.BeginDrawing();

                Raylib.ClearBackground(Color.BLACK);

                Raylib.BeginTextureMode(_target);

                Raylib.ClearBackground(Color.BLACK);

                ScreenManager.Instance.Draw();

                Raylib.EndTextureMode();

                var sourceRec = new Rectangle(
                    0.0f,
                    0.0f,
                    _target.texture.width,
                    -_target.texture.height
                );

                var destRec = new Rectangle(
                    (Raylib.GetScreenWidth() - (_gameWidth * scale)) * 0.5f,
                    (Raylib.GetScreenHeight() - (_gameHeight * scale)) * 0.5f,
                    _gameWidth * scale,
                    _gameHeight * scale
                );

                Raylib.DrawTexturePro(_target.texture, sourceRec, destRec, new Vector2(0, 0), 0.0f, Color.WHITE);

                _controller.Draw();

                Raylib.EndDrawing();
            }

            _controller.Dispose();

            Raylib.UnloadRenderTexture(_target);

            Raylib.CloseAudioDevice();

            Raylib.CloseWindow();
        }
    }
}
