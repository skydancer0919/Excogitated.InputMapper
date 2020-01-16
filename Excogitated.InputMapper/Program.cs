using System;
using System.Windows;

namespace Excogitated.InputMapper
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            using var mw = new MainWindow();
            new Application().Run(mw);
        }
    }
}
