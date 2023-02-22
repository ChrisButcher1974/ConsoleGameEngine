1) Create a console app and add the console engine in as a project dependency
2) Create a base class and inherit the ConsoleGame Engine
3) implement the 2 abstract classes
4) Create and instance of the class from step 2. Code should look like this:

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
	public override bool OnUserCreate()
        {
            return true;
        }

	public override bool OnUserUpdate(float fElapsedTime)
        {
		return true;
	}
}

5) OnUserCreate is used to setup the user in the game, OnUserUpdate is called via a loop to update the sreen. All game logic shoudl go into the OnUSerUpdate
6) Keybooard Input is handled and can be accessed as below:
	if (keys[(int)ConsoleKey.LeftArrow].bHeld)
            {
                //Do Something
            }
7) Some shapes are already in the GameEngine (Triangles, Lines and Circles). Fill draws a fille Rectangle, Draw draws a character.