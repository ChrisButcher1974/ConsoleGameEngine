using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consoleGameEngine.Classes
{
    public class KeyState
    {
        public KeyState()
        {
            this.bPressed = false;
            this.bHeld= false;
            this.bReleased = false;
        }

        public bool bPressed;
        public bool bReleased;
        public bool bHeld;
    }
}
