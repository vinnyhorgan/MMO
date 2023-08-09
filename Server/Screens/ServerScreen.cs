using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Riptide;

namespace Server.Screens
{
    class ServerScreen : Screen
    {
        public override void Draw()
        {
            Raylib.DrawTextEx(Game.Instance.Font, $"Server running on port: {NetworkManager.Instance.Server.Port}", new Vector2(10, 10), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Server max clients: {NetworkManager.Instance.Server.MaxClientCount}", new Vector2(10, 30), Game.Instance.Font.baseSize, 0, Color.WHITE);
            Raylib.DrawTextEx(Game.Instance.Font, $"Connected clients: {NetworkManager.Instance.Server.ClientCount}", new Vector2(10, 50), Game.Instance.Font.baseSize, 0, Color.WHITE);

            for (int i = 0; i < NetworkManager.Instance.Server.ClientCount; i++)
            {
                Raylib.DrawTextEx(Game.Instance.Font, $"Client {NetworkManager.Instance.Server.Clients[i].Id}", new Vector2(10, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.WHITE);

                if (NetworkManager.Instance.Server.Clients[i].IsConnected)
                {
                    Raylib.DrawTextEx(Game.Instance.Font, "Connected", new Vector2(200, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.GREEN);
                }
                else
                {
                    Raylib.DrawTextEx(Game.Instance.Font, "Disconnected", new Vector2(200, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.RED);
                }

                Raylib.DrawTextEx(Game.Instance.Font, $"Ping: {NetworkManager.Instance.Server.Clients[i].SmoothRTT}", new Vector2(400, 100 + (i * 20)), Game.Instance.Font.baseSize, 0, Color.WHITE);
            }
        }
    }
}
