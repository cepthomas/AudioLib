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
            int sout = SampleOps.SnapSample(1234567, SnapType.None);
            UT_EQUAL(sout, 1234567);

            sout = SampleOps.SnapSample(1234367, SnapType.Fine); // round down
            UT_EQUAL(sout, 1234000);

            sout = SampleOps.SnapSample(1234567, SnapType.Fine); // round up
            UT_EQUAL(sout, 1235000);

            sout = SampleOps.SnapSample(1234567, SnapType.Coarse); // round down
            UT_EQUAL(sout, 1230000);

            sout = SampleOps.SnapSample(1235567, SnapType.Coarse); // round up
            UT_EQUAL(sout, 1240000);
        }
    }

    public class CONVERT_TIME : TestSuite
    {
        public override void RunSuite()
        {
            // AudioTime <-> sample.
            int sample1 = 25354545;
            int sample2 = 1904788;
            int errmax = 25; // Roundtrip conversion error max.

            UT_EQUAL(TimeOps.Format(sample1), "09.34.933");
            int sout = TimeOps.TextToSample("9.34.933");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            UT_LESS(err, errmax);

            UT_EQUAL(TimeOps.Format(sample2), "00.43.192");
            sout = TimeOps.TextToSample("00.43.192");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            UT_LESS(err, errmax);

            sout = TimeOps.SnapSample(sample1, SnapType.Fine); // round down
            UT_EQUAL(TimeOps.Format(sout), "09.34.900");

            sout = TimeOps.SnapSample(sample2, SnapType.Fine); // round up
            UT_EQUAL(TimeOps.Format(sout), "00.43.200");

            sout = TimeOps.SnapSample(sample1, SnapType.Coarse); // round up
            UT_EQUAL(TimeOps.Format(sout), "09.35.000");

            sout = TimeOps.SnapSample(sample2, SnapType.Coarse); // round down
            UT_EQUAL(TimeOps.Format(sout), "00.43.000");

            // Parsing.
            // Good ones.
            sout = TimeOps.TextToSample("8.47.123");
            UT_EQUAL(sout, 23246124);
            sout = TimeOps.TextToMsec("7.47.123");
            UT_EQUAL(sout, 467123);

            // Bad ones.
            sout = TimeOps.TextToSample("1:2");
            UT_EQUAL(sout, -1);
            sout = TimeOps.TextToSample("1.2");
            UT_EQUAL(sout, -1);
            sout = TimeOps.TextToSample("1.2.9999");
            UT_EQUAL(sout, -1);
            sout = TimeOps.TextToSample("dsdsd");
            UT_EQUAL(sout, -1);
        }
    }

    public class CONVERT_BARBEAT : TestSuite
    {
        public override void RunSuite()
        {
            // BarBeat <-> sample.
            int sample1 = 80126934;
            int sample2 = 9055612;
            int errmax = 100; // Roundtrip conversion error max.

            BarBeatOps.BPM = 76.58f;

            UT_EQUAL(BarBeatOps.Format(sample1), "579.3.02");
            int sout = BarBeatOps.TextToSample("579.3.02");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"BarBeat error:{err}");
            UT_LESS(err, errmax);

            BarBeatOps.BPM = 117.02f;

            UT_EQUAL(BarBeatOps.Format(sample2), "100.0.49");
            sout = BarBeatOps.TextToSample("100.0.49");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"BarBeat error:{err}");
            UT_LESS(err, errmax);

            sout = BarBeatOps.TextToSample("421.2.49");
            sout = BarBeatOps.SnapSample(sout, SnapType.Fine); // round beat down
            UT_EQUAL(BarBeatOps.Format(sout), "421.2.00");

            sout = BarBeatOps.TextToSample("421.2.51");
            sout = BarBeatOps.SnapSample(sout, SnapType.Fine); // round beat up
            UT_EQUAL(BarBeatOps.Format(sout), "421.3.00");

            sout = BarBeatOps.TextToSample("421.3.99");
            sout = BarBeatOps.SnapSample(sout, SnapType.Fine); // round beat up
            UT_EQUAL(BarBeatOps.Format(sout), "422.0.00");

            sout = BarBeatOps.TextToSample("421.3.88");
            sout = BarBeatOps.SnapSample(sout, SnapType.Coarse); // round bar up
            UT_EQUAL(BarBeatOps.Format(sout), "422.0.00");

            sout = BarBeatOps.TextToSample("421.1.48");
            sout = BarBeatOps.SnapSample(sout, SnapType.Coarse); // round bar down
            UT_EQUAL(BarBeatOps.Format(sout), "421.0.00");

            // Parsing.
            // Good one.
            sout = BarBeatOps.TextToSample("32.2.78");
            UT_EQUAL(BarBeatOps.Format(sout), "32.2.78");

            // Bad ones.
            sout = BarBeatOps.TextToSample("1:2");
            UT_EQUAL(sout, -1);

            sout = BarBeatOps.TextToSample("1.2");
            UT_EQUAL(sout, -1);

            sout = BarBeatOps.TextToSample("1.2.9999");
            UT_EQUAL(sout, -1);

            sout = BarBeatOps.TextToSample("dsdsd");
            UT_EQUAL(sout, -1);
        }
    }
}
