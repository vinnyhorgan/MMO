namespace Server
{
    class ScreenManager
    {
        private static ScreenManager _instance;

        private Screen _currentScreen;

        public static ScreenManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScreenManager();
                }

                return _instance;
            }
        }

        public void SetScreen(Screen screen)
        {
            _currentScreen?.Unload();
            _currentScreen = screen;
            _currentScreen?.Load();
        }

        public T GetScreen<T>() where T : Screen
        {
            return _currentScreen as T;
        }

        public void Update(float dt)
        {
            _currentScreen?.Update(dt);
        }

        public void Draw()
        {
            _currentScreen?.Draw();
        }
    }
}
