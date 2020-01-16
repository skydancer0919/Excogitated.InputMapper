using Excogitated.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsInput;
using WindowsInput.Native;

namespace Excogitated.InputMapper
{
    internal class MainWindow : Window, IDisposable
    {
        private readonly CowDictionary<MouseAction, List<InputMapping>> _mappings = new CowDictionary<MouseAction, List<InputMapping>>();
        private readonly InputSimulator _input;
        private readonly HookMouse _mouse;
        private readonly Task _init;

        public MainWindow()
        {
            Title = "Input Mapper";
            Content = new Grid
            {

            };

            _input = new InputSimulator();
            _mouse = new HookMouse();
            _init = Task.Run(async () =>
            {
                var file = new FileInfo("./mappings.json");
                if (file.Exists)
                {
                    using var stream = File.OpenRead("./mappings.json");
                    var mappings = await Jsonizer.DeserializeAsync<List<InputMapping>>(stream);
                    foreach (var m in mappings)
                        _mappings.GetOrAdd(m.Trigger).Add(m);
                }
                _mappings.GetOrAdd(MouseAction.XButton2Up).Add(new InputMapping
                {
                    Trigger = MouseAction.XButton2Up,
                    Modifiers = new[] { VirtualKeyCode.CONTROL },
                    Keys = new[] { VirtualKeyCode.VK_W },
                    IncludedFiles = new[] { "chrome.exe" },
                    ExcludedWindowNames = new[] { "Play - Stadia" },
                });
                _mappings.GetOrAdd(MouseAction.XButton1Up).Add(new InputMapping
                {
                    Trigger = MouseAction.XButton1Up,
                    Modifiers = new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT },
                    Keys = new[] { VirtualKeyCode.LBUTTON },
                    IncludedFiles = new[] { "chrome.exe" },
                    ExcludedWindowNames = new[] { "Play - Stadia" },
                });
                _mouse.On += _mouse_On;
            });
        }

        private void _mouse_On(MouseEvent obj)
        {
            var name = WindowInfo.GetActiveWindowName();
            var file = WindowInfo.GetActiveWindowFile();
            foreach (var m in _mappings.GetOrAdd(obj.Action))
                if (IsMatch(name, file, m))
                    _input.Keyboard.ModifiedKeyStroke(m.Modifiers, m.Keys);
        }

        private static bool IsMatch(string name, string file, InputMapping mapping)
        {
            if (mapping.ExcludedWindowNames != null)
                foreach (var excludedName in mapping.ExcludedWindowNames)
                    if (name.Contains(excludedName))
                        return false;
            if (mapping.IncludedFiles != null)
                foreach (var includedFile in mapping.IncludedFiles)
                    if (file.Contains(includedFile))
                        return true;
            return true;
        }

        public void Dispose()
        {
            _mouse.Dispose();
        }
    }

    public class InputMapping
    {
        public MouseAction Trigger { get; set; }
        public VirtualKeyCode[] Modifiers { get; internal set; }
        public VirtualKeyCode[] Keys { get; internal set; }
        public string[] IncludedFiles { get; internal set; }
        public string[] ExcludedWindowNames { get; internal set; }
    }
}
