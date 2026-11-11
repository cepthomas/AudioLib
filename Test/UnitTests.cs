using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib.Test
{
    public class CONVERT_SAMPLE : TestSuite
    {
        public override void RunSuite()
        {
            // Sample <-> sample.
            var conv = Converter.Create(WaveSelectionMode.Sample);

            int sout = conv.Snap(1234567, SnapType.None);
            Assert(sout == 1234567);

            sout = conv.Snap(1234367, SnapType.Fine); // round down
            Assert(sout == 1234000);

            sout = conv.Snap(1234567, SnapType.Fine); // round up
            Assert(sout == 1235000);

            sout = conv.Snap(1234567, SnapType.Coarse); // round down
            Assert(sout == 1230000);

            sout = conv.Snap(1235567, SnapType.Coarse); // round up
            Assert(sout == 1240000);
        }
    }

    public class CONVERT_TIME : TestSuite
    {
        public override void RunSuite()
        {
            // AudioTime <-> sample.
            var conv = Converter.Create(WaveSelectionMode.Time);

            int sample1 = 25354545;
            int sample2 = 1904788;
            int errmax = 25; // Roundtrip conversion error max.

            Assert(conv.Format(sample1) == "09.34.933");
            int sout = conv.Parse("9.34.933");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            Assert(err < errmax);

            Assert(conv.Format(sample2) == "00.43.192");
            sout = conv.Parse("00.43.192");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            Assert(err < errmax);

            sout = conv.Snap(sample1, SnapType.Fine); // round down
            Assert(conv.Format(sout) == "09.34.900");

            sout = conv.Snap(sample2, SnapType.Fine); // round up
            Assert(conv.Format(sout) == "00.43.200");

            sout = conv.Snap(sample1, SnapType.Coarse); // round up
            Assert(conv.Format(sout) == "09.35.000");

            sout = conv.Snap(sample2, SnapType.Coarse); // round down
            Assert(conv.Format(sout) == "00.43.000");

            // Parsing.
            // Good ones.
            sout = conv.Parse("8.47.123");
            Assert(sout == 23246124);
            sout = conv.Parse("7.47.123");
            Assert(sout == 20600124);

            // Bad ones.
            sout = conv.Parse("1:2");
            Assert(sout == -1);
            sout = conv.Parse("1.2");
            Assert(sout == -1);
            sout = conv.Parse("1.2.9999");
            Assert(sout == -1);
            sout = conv.Parse("dsdsd");
            Assert(sout == -1);
        }
    }

    public class CONVERT_BAR : TestSuite
    {
        public override void RunSuite()
        {
            // Bar <-> sample.
            int sample1 = 80126934;
            int sample2 = 9055612;
            int errmax = 100; // Roundtrip conversion error max.

            var conv = Converter.Create(WaveSelectionMode.Bar);
            conv.BPM = 76.58f;

            Assert(conv.Format(sample1) == "579.3.02");
            int sout = conv.Parse("579.3.02");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"Bar error:{err}");
            Assert(err < errmax);

            conv.BPM = 117.02f;

            Assert(conv.Format(sample2) == "100.0.49");
            sout = conv.Parse("100.0.49");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"Bar error:{err}");
            Assert(err < errmax);

            sout = conv.Parse("421.2.49");
            sout = conv.Snap(sout, SnapType.Fine); // round beat down
            Assert(conv.Format(sout) == "421.2.00");

            sout = conv.Parse("421.2.51");
            sout = conv.Snap(sout, SnapType.Fine); // round beat up
            Assert(conv.Format(sout) == "421.3.00");

            sout = conv.Parse("421.3.99");
            sout = conv.Snap(sout, SnapType.Fine); // round beat up
            Assert(conv.Format(sout) == "422.0.00");

            sout = conv.Parse("421.3.88");
            sout = conv.Snap(sout, SnapType.Coarse); // round bar up
            Assert(conv.Format(sout) == "422.0.00");

            sout = conv.Parse("421.1.48");
            sout = conv.Snap(sout, SnapType.Coarse); // round bar down
            Assert(conv.Format(sout) == "421.0.00");

            // Parsing.
            // Good one.
            sout = conv.Parse("32.2.78");
            Assert(conv.Format(sout) == "32.2.78");

            // Bad ones.
            sout = conv.Parse("1:2");
            Assert(sout == -1);

            sout = conv.Parse("1.2");
            Assert(sout == -1);

            sout = conv.Parse("1.2.9999");
            Assert(sout == -1);

            sout = conv.Parse("dsdsd");
            Assert(sout == -1);
        }
    }
}
