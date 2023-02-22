using consoleGameEngine.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace consoleGameEngine
{
    public abstract partial class ConsoleGameEngine
    {
        public int iScreenWidth = 80;
        public int iScreenHeight = 30;
        private SMALL_RECT rectWindow;
        private IntPtr hConsole;
        private IntPtr hConsoleIn;
        private string sAppName;
        private bool bRunning;
        private uint bytesWritten;
        public List<KeyState> keys = Enumerable.Range(0, 256).Select(x => new KeyState()).ToList();
        private List<short> keyNewState = new List<short>(new short[256]);
        private List<short> keyOldState = new List<short>(new short[256]);

        public CHAR_INFO[] bufScreen;
        public ConsoleGameEngine()
        {
            iScreenWidth = 80;
            iScreenHeight = 30;

            hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
            hConsoleIn = GetStdHandle(STD_INPUT_HANDLE);

            sAppName = "Default";
        }

        public abstract bool OnUserCreate();
        public abstract bool OnUserUpdate(float fElapsedTime);


        public int ConstructConsole(int width, int height, string sAppName,
            short fontw = 12, short fonth = 12)
        {
            if (hConsole == INVALID_HANDLE_VALUE) return Error("Bad Handle");
            var a = new KeyState();
            iScreenWidth = width;
            iScreenHeight = height;

            this.sAppName = sAppName;

            rectWindow = new SMALL_RECT()
            {
                Left = 0,
                Top = 0,
                Bottom = 1,
                Right = 1
            };

            SetConsoleWindowInfo(hConsole, true, ref rectWindow);

            COORD coord = new COORD()
            {
                X = (short)iScreenWidth,
                Y = (short)iScreenHeight
            };

            if (!SetConsoleScreenBufferSize(hConsole, coord))
                Error("SetConsoleScreenBufferSize");

            // Assign screen buffer to the console
            if (!SetConsoleActiveScreenBuffer(hConsole))
                return Error("SetConsoleActiveScreenBuffer");

            CONSOLE_FONT_INFO_EX cfi = new CONSOLE_FONT_INFO_EX();
            cfi.cbSize = (uint)Marshal.SizeOf(cfi);
            cfi.nFont = 0;
            cfi.dwFontSize.X = fontw;
            cfi.dwFontSize.Y = fonth;
            cfi.FontFamily = (int)FontPitchAndFamily.FF_DONTCARE;
            cfi.FontWeight = (int)FontWeight.FW_NORMAL;
            cfi.FaceName = "Consolas";

            if (!SetCurrentConsoleFontEx(hConsole, false, cfi))
                return Error("SetCurrentConsoleFontEx");

            CONSOLE_SCREEN_BUFFER_INFO csbi;

            if (!GetConsoleScreenBufferInfo(hConsole, out csbi))
                return Error("GetConsoleScreenBufferInfo");
            if (iScreenHeight > csbi.dwMaximumWindowSize.Y)
                return Error("Screen Height / Font Height Too Big");
            if (iScreenWidth > csbi.dwMaximumWindowSize.X)
                return Error("Screen Width / Font Width Too Big");

            rectWindow = new SMALL_RECT()
            {
                Left = 0,
                Top = 0,
                Right = (short)(iScreenWidth - 1),
                Bottom = (short)(iScreenHeight - 1)
            };

            if (!SetConsoleWindowInfo(hConsole, true, ref rectWindow))
                return Error("SetConsoleWindowInfo");

            // Set flags to allow mouse input		
            if (!SetConsoleMode(hConsoleIn,
                (uint)ConsoleModes.ENABLE_EXTENDED_FLAGS |
                (uint)ConsoleModes.ENABLE_WINDOW_INPUT |
                (uint)ConsoleModes.ENABLE_MOUSE_INPUT))
                return Error("SetConsoleMode");

            bufScreen = Enumerable.Range(0, iScreenWidth * iScreenHeight).Select(x => new CHAR_INFO()).ToArray();

            return 1;
        }

        public void Start()
        {
            bRunning = true;

            Thread gameThread = new Thread(GameStart);
            gameThread.Start();
            gameThread.Join();
        }

        private void GameStart()
        {
            if (!OnUserCreate())
                return;

            DateTime t1 = DateTime.Now;
            DateTime t2 = DateTime.Now;

            while (bRunning)
            {
                t2 = DateTime.Now;
                var elapsedTime = (t2 - t1);
                t1 = t2;
                var fElapsedTime = (float)elapsedTime.TotalMilliseconds / 1000;

                for (int i = 0; i < 256; i++)
                {
                    keyNewState[i] = GetAsyncKeyState(i);

                    keys[i].bPressed = false;
                    keys[i].bReleased = false;

                    if (keyNewState[i] != keyOldState[i])
                    {
                        if (keyNewState[i] == Int16.MinValue)
                        {
                            keys[i].bPressed = !keys[i].bHeld;
                            keys[i].bHeld = true;
                        }
                        else
                        {
                            keys[i].bReleased = true;
                            keys[i].bHeld = false;
                        }
                    }

                    keyOldState[i] = keyNewState[i];
                }

                if (!OnUserUpdate(fElapsedTime))
                    bRunning = false;

                var title = $"Console Game Engine - {sAppName} - FPS: {1.0f / fElapsedTime}";

                SetConsoleTitle(title);

                WriteConsoleOutput(hConsole, bufScreen,
                    new COORD() { X = (short)iScreenWidth, Y = (short)iScreenHeight },
                    new COORD() { X = 0, Y = 0 }, ref rectWindow);
            }
        }

        public virtual void Draw(int x, int y, short c = 219, short col = 0x000F)
        {
            if (x >= 0 && x < iScreenWidth && y >= 0 && y < iScreenHeight)
            {
                bufScreen[y * iScreenWidth + x].UnicodeChar = (char)c;
                bufScreen[y * iScreenWidth + x].Attributes = (ushort)col;
            }
        }

        public void DrawRect(int x, int y, int w, int h, short c = 219, short col = 0x000F)
        {
            DrawLine(x, y, x + w, y, c, col);
            DrawLine(x + w, y, x + w, y + h, c, col);
            DrawLine(x + w, y + h, x, y + h, c, col);
            DrawLine(x, y + h, x, y, c, col);
        }

        public void Fill(int x1, int y1, int x2, int y2, short c = 219, short col = 0x000F)
        {
            Clip(x1, y1);
            Clip(x2, y2);
            for (int x = x1; x < x2; x++)
                for (int y = y1; y < y2; y++)
                    Draw(x, y, c, col);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, short c = 219, short col = 0x000F)
        {
            int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
            dx = x2 - x1; dy = y2 - y1;
            dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
            px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
            if (dy1 <= dx1)
            {
                if (dx >= 0)
                { x = x1; y = y1; xe = x2; }
                else
                { x = x2; y = y2; xe = x1; }

                Draw(x, y, c, col);

                for (i = 0; x < xe; i++)
                {
                    x = x + 1;
                    if (px < 0)
                        px = px + 2 * dy1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y = y - 1;
                        px = px + 2 * (dy1 - dx1);
                    }
                    Draw(x, y, c, col);
                }
            }
            else
            {
                if (dy >= 0)
                { x = x1; y = y1; ye = y2; }
                else
                { x = x2; y = y2; ye = y1; }

                Draw(x, y, c, col);

                for (i = 0; y < ye; i++)
                {
                    y = y + 1;
                    if (py <= 0)
                        py = py + 2 * dx1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x = x - 1;
                        py = py + 2 * (dx1 - dy1);
                    }
                    Draw(x, y, c, col);
                }
            }
        }

        public void DrawCircle(int xc, int yc, int r, short c = 219, short col = 0x000F)
        {
            int x = 0;
            int y = r;
            int p = 3 - 2 * r;
            //if (!r) return;

            while (y >= x) // only formulate 1/8 of circle
            {
                Draw(xc - x, yc - y, c, col);//upper left left
                Draw(xc - y, yc - x, c, col);//upper upper left
                Draw(xc + y, yc - x, c, col);//upper upper right
                Draw(xc + x, yc - y, c, col);//upper right right
                Draw(xc - x, yc + y, c, col);//lower left left
                Draw(xc - y, yc + x, c, col);//lower lower left
                Draw(xc + y, yc + x, c, col);//lower lower right
                Draw(xc + x, yc + y, c, col);//lower right right
                if (p < 0) p += 4 * x++ + 6;
                else p += 4 * (x++ - y--) + 10;
            }
        }

        void Clip(int x, int y)
        {
            if (x < 0) x = 0;
            if (x >= iScreenWidth) x = iScreenWidth;
            if (y < 0) y = 0;
            if (y >= iScreenHeight) y = iScreenHeight;
        }

        public void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, short c = 219, short col = 0x000F)
        {
            DrawLine(x1, y1, x2, y2, c, col);
            DrawLine(x2, y2, x3, y3, c, col);
            DrawLine(x3, y3, x1, y1, c, col);
        }

        protected int Error(string msg)
        {
            //DO SOMETHING
            return 0;
        }
    }
}