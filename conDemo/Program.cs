using consoleGameEngine;

namespace conDemo
{
    internal class Program
    {
        static int iScreenWidth = 400;
        static int iScreenHeight = 240;
        static short iblockSize = 4;

        static void Main(string[] args)
        {
            Game cge = new Game();

            cge.ConstructConsole(iScreenWidth, iScreenHeight,
                "Console Demo", iblockSize, iblockSize);

            cge.Start();
        }
    }

    public class Game : ConsoleGameEngine
    {
        float fPlayerX;
        float fPlayerY;

        public override bool OnUserCreate()
        {
            fPlayerX = 10f;
            fPlayerY = 10f;
            return true;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            if (keys[(int)ConsoleKey.LeftArrow].bHeld)
            {
                fPlayerX -= 15.0f * fElapsedTime;
            }
            if (keys[(int)ConsoleKey.RightArrow].bHeld)
            {
                fPlayerX += 15.0f * fElapsedTime;
            }
            if (keys[(int)ConsoleKey.UpArrow].bHeld)
            {
                fPlayerY -= 15.0f * fElapsedTime;
            }
            if (keys[(int)ConsoleKey.DownArrow].bHeld)
            {
                fPlayerY += 15.0f * fElapsedTime;
            }

            Fill(0, 0, iScreenWidth, iScreenHeight, (short)' ', 0);

            DrawRect(0, 0, iScreenWidth - 1, iScreenHeight - 1);

            Fill((int)fPlayerX, (int)fPlayerY, (int)fPlayerX + 5, (int)fPlayerY + 5, (short)' ', 0x0090);

            DrawLine(30, 20, 35, 69);

            DrawCircle(120, 120, 80);

            return true;
        }

        private void RandomChars()
        {
            var rnd = new Random();
            for (int x = 0; x < iScreenWidth; x++)
            {
                for (int y = 0; y < iScreenHeight; y++)
                {
                    Draw(x, y, (short)219, (short)(rnd.Next() % 16));
                }
            }
        }
    }
}