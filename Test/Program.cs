using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Ephemera.AudioLib.Test
{
    class Program
    {
        static void Main(string[] _)
        {
            // Use test host for debugging UI components.
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestHost());
        }
    }
}
