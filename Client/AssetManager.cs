using System.IO;
using System.Reflection;
using Raylib_cs;

namespace Client
{
    class AssetManager
    {
        private static AssetManager _instance;

        public static AssetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetManager();
                }

                return _instance;
            }
        }

        public Texture2D LoadTexture(string name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Client.Assets.Textures.{name}"))
            {
                if (stream == null)
                {
                    Logger.Error($"Could not find texture {name}");
                    return new Texture2D();
                }

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);

                    var image = Raylib.LoadImageFromMemory(".png", ms.ToArray());
                    var texture = Raylib.LoadTextureFromImage(image);
                    Raylib.UnloadImage(image);

                    return texture;
                }
            }
        }

        public Font LoadFont(string name, int size)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Client.Assets.Fonts.{name}"))
            {
                if (stream == null)
                {
                    Logger.Error($"Could not find font {name}");
                    return new Font();
                }

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);

                    var font = Raylib.LoadFontFromMemory(".ttf", ms.ToArray(), size, null, 0);
                    return font;
                }
            }
        }
    }
}
