using NBagOfTricks.PNUT;
using NBagOfTricks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace AudioLib.Test
{
    public class CONVERT_SAMPLE : TestSuite
    {
        public override void RunSuite()
        {
            // Sample <-> sample.
            SampleOps ops = new();

            int sout = ops.Snap(1234567, SnapType.None);
            UT_EQUAL(sout, 1234567);

            sout = ops.Snap(1234367, SnapType.Fine); // round down
            UT_EQUAL(sout, 1234000);

            sout = ops.Snap(1234567, SnapType.Fine); // round up
            UT_EQUAL(sout, 1235000);

            sout = ops.Snap(1234567, SnapType.Coarse); // round down
            UT_EQUAL(sout, 1230000);

            sout = ops.Snap(1235567, SnapType.Coarse); // round up
            UT_EQUAL(sout, 1240000);
        }
    }

    public class CONVERT_TIME : TestSuite
    {
        public override void RunSuite()
        {
            // AudioTime <-> sample.
            TimeOps ops = new();

            int sample1 = 25354545;
            int sample2 = 1904788;
            int errmax = 25; // Roundtrip conversion error max.

            UT_EQUAL(ops.Format(sample1), "09.34.933");
            int sout = ops.Parse("9.34.933");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            UT_LESS(err, errmax);

            UT_EQUAL(ops.Format(sample2), "00.43.192");
            sout = ops.Parse("00.43.192");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            UT_LESS(err, errmax);

            sout = ops.Snap(sample1, SnapType.Fine); // round down
            UT_EQUAL(ops.Format(sout), "09.34.900");

            sout = ops.Snap(sample2, SnapType.Fine); // round up
            UT_EQUAL(ops.Format(sout), "00.43.200");

            sout = ops.Snap(sample1, SnapType.Coarse); // round up
            UT_EQUAL(ops.Format(sout), "09.35.000");

            sout = ops.Snap(sample2, SnapType.Coarse); // round down
            UT_EQUAL(ops.Format(sout), "00.43.000");

            // Parsing.
            // Good ones.
            sout = ops.Parse("8.47.123");
            UT_EQUAL(sout, 23246124);
            sout = ops.Parse("7.47.123");
            UT_EQUAL(sout, 20600124);

            // Bad ones.
            sout = ops.Parse("1:2");
            UT_EQUAL(sout, -1);
            sout = ops.Parse("1.2");
            UT_EQUAL(sout, -1);
            sout = ops.Parse("1.2.9999");
            UT_EQUAL(sout, -1);
            sout = ops.Parse("dsdsd");
            UT_EQUAL(sout, -1);
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

            BarOps ops = new();

            Globals.BPM = 76.58f;

            UT_EQUAL(ops.Format(sample1), "579.3.02");
            int sout = ops.Parse("579.3.02");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"Bar error:{err}");
            UT_LESS(err, errmax);

            Globals.BPM = 117.02f;

            UT_EQUAL(ops.Format(sample2), "100.0.49");
            sout = ops.Parse("100.0.49");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"Bar error:{err}");
            UT_LESS(err, errmax);

            sout = ops.Parse("421.2.49");
            sout = ops.Snap(sout, SnapType.Fine); // round beat down
            UT_EQUAL(ops.Format(sout), "421.2.00");

            sout = ops.Parse("421.2.51");
            sout = ops.Snap(sout, SnapType.Fine); // round beat up
            UT_EQUAL(ops.Format(sout), "421.3.00");

            sout = ops.Parse("421.3.99");
            sout = ops.Snap(sout, SnapType.Fine); // round beat up
            UT_EQUAL(ops.Format(sout), "422.0.00");

            sout = ops.Parse("421.3.88");
            sout = ops.Snap(sout, SnapType.Coarse); // round bar up
            UT_EQUAL(ops.Format(sout), "422.0.00");

            sout = ops.Parse("421.1.48");
            sout = ops.Snap(sout, SnapType.Coarse); // round bar down
            UT_EQUAL(ops.Format(sout), "421.0.00");

            // Parsing.
            // Good one.
            sout = ops.Parse("32.2.78");
            UT_EQUAL(ops.Format(sout), "32.2.78");

            // Bad ones.
            sout = ops.Parse("1:2");
            UT_EQUAL(sout, -1);

            sout = ops.Parse("1.2");
            UT_EQUAL(sout, -1);

            sout = ops.Parse("1.2.9999");
            UT_EQUAL(sout, -1);

            sout = ops.Parse("dsdsd");
            UT_EQUAL(sout, -1);
        }
    }
}
