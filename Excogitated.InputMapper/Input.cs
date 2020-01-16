using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Excogitated.InputMapper
{
    public class Input
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        //these are virtual key codes for keys
        const int VK_UP = 0x26; //up key
        const int VK_DOWN = 0x28;  //down key
        const int VK_LEFT = 0x25;
        const int VK_RIGHT = 0x27;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        public int press()
        {
            
            //Press the key
            keybd_event(VK_UP, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            return 0;

        }

        public int release()
        {
            //Release the key
            keybd_event(VK_RIGHT, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            return 0;
        }
    }
}
