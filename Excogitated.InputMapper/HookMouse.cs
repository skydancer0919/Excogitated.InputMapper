using Excogitated.Common;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Excogitated.InputMapper
{
    public class HookMouse : IDisposable
    {
        private readonly LowLevelMouseProc _proc;
        private readonly IntPtr _hookID;

        public event Action<MouseEvent> On;

        public HookMouse()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
            On = null;
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var button = (MouseAction)wParam;
            if (nCode >= 0 && button != MouseAction.Move)
                Task.Run(() =>
                {
                    var msg = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    var rawButton = wParam.ToInt32() + msg.mouseData;
                    On?.Invoke(new MouseEvent
                    {
                        X = msg.pt.x,
                        Y = msg.pt.y,
                        Time = new DateTime(msg.time),
                        Flags = msg.flags,
                        Data = msg.mouseData,
                        Action = (MouseAction)rawButton
                    });
                });
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public enum MouseAction : long
    {
        Move = 512,
        LeftDown = 513,
        LeftUp = 514,
        RightDown = 516,
        RightUp = 517,
        WheelUp = 7864842,
        WheelDown = 4287103498,
        XButton1Down = 66059,
        XButton1Up = 66060,
        XButton2Down = 131595,
        XButton2Up = 131596,
    }

    public struct MouseEvent
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public DateTime Time { get; internal set; }
        public uint Flags { get; internal set; }
        public uint Data { get; internal set; }
        public MouseAction Action { get; internal set; }
    }
}
