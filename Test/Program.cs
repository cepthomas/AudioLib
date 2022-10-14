using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Ephemera.AudioLib.Test
{
    class Program
    {
        static void Main(string[] _)
        {
            // Use test host for debugging UI components.
            TestHost w = new();
            w.ShowDialog();
        }
    }
}
