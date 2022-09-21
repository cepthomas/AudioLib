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
            int sout = Converters.SnapSample(1234567, SnapType.None);
            UT_EQUAL(sout, 1234567);

            sout = Converters.SnapSample(1234367, SnapType.Fine); // round down
            UT_EQUAL(sout, 1234000);

            sout = Converters.SnapSample(1234567, SnapType.Fine); // round up
            UT_EQUAL(sout, 1235000);

            sout = Converters.SnapSample(1234567, SnapType.Coarse); // round down
            UT_EQUAL(sout, 1230000);

            sout = Converters.SnapSample(1235567, SnapType.Coarse); // round up
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

            UT_EQUAL(AudioTime.Format(sample1), "09.34.933");
            int sout = AudioTime.TextToSample("9.34.933");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            UT_LESS(err, errmax);

            UT_EQUAL(AudioTime.Format(sample2), "00.43.192");
            sout = AudioTime.TextToSample("00.43.192");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"AudioTime error:{err}");
            UT_LESS(err, errmax);

            sout = AudioTime.SnapSample(sample1, SnapType.Fine); // round down
            UT_EQUAL(AudioTime.Format(sout), "09.34.900");

            sout = AudioTime.SnapSample(sample2, SnapType.Fine); // round up
            UT_EQUAL(AudioTime.Format(sout), "00.43.200");

            sout = AudioTime.SnapSample(sample1, SnapType.Coarse); // round up
            UT_EQUAL(AudioTime.Format(sout), "09.35.000");

            sout = AudioTime.SnapSample(sample2, SnapType.Coarse); // round down
            UT_EQUAL(AudioTime.Format(sout), "00.43.000");

            // Parsing.
            // Good ones.
            sout = AudioTime.TextToSample("8.47.123");
            UT_EQUAL(sout, 23246124);
            sout = AudioTime.TextToMsec("7.47.123");
            UT_EQUAL(sout, 467123);

            // Bad ones.
            sout = AudioTime.TextToSample("1:2");
            UT_EQUAL(sout, -1);
            sout = AudioTime.TextToSample("1.2");
            UT_EQUAL(sout, -1);
            sout = AudioTime.TextToSample("1.2.9999");
            UT_EQUAL(sout, -1);
            sout = AudioTime.TextToSample("dsdsd");
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

            BarBeat.BPM = 76.58f;

            UT_EQUAL(BarBeat.Format(sample1), "579.3.02");
            int sout = BarBeat.TextToSample("579.3.02");
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"BarBeat error:{err}");
            UT_LESS(err, errmax);

            BarBeat.BPM = 117.02f;

            UT_EQUAL(BarBeat.Format(sample2), "100.0.49");
            sout = BarBeat.TextToSample("100.0.49");
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"BarBeat error:{err}");
            UT_LESS(err, errmax);

            sout = BarBeat.TextToSample("421.2.49");
            sout = BarBeat.SnapSample(sout, SnapType.Fine); // round beat down
            UT_EQUAL(BarBeat.Format(sout), "421.2.00");

            sout = BarBeat.TextToSample("421.2.51");
            sout = BarBeat.SnapSample(sout, SnapType.Fine); // round beat up
            UT_EQUAL(BarBeat.Format(sout), "421.3.00");

            sout = BarBeat.TextToSample("421.3.99");
            sout = BarBeat.SnapSample(sout, SnapType.Fine); // round beat up
            UT_EQUAL(BarBeat.Format(sout), "422.0.00");

            sout = BarBeat.TextToSample("421.3.88");
            sout = BarBeat.SnapSample(sout, SnapType.Coarse); // round bar up
            UT_EQUAL(BarBeat.Format(sout), "422.0.00");

            sout = BarBeat.TextToSample("421.1.48");
            sout = BarBeat.SnapSample(sout, SnapType.Coarse); // round bar down
            UT_EQUAL(BarBeat.Format(sout), "421.0.00");

            // Parsing.
            // Good one.
            sout = BarBeat.TextToSample("32.2.78");
            UT_EQUAL(BarBeat.Format(sout), "32.2.78");

            // Bad ones.
            sout = BarBeat.TextToSample("1:2");
            UT_EQUAL(sout, -1);

            sout = BarBeat.TextToSample("1.2");
            UT_EQUAL(sout, -1);

            sout = BarBeat.TextToSample("1.2.9999");
            UT_EQUAL(sout, -1);

            sout = BarBeat.TextToSample("dsdsd");
            UT_EQUAL(sout, -1);
        }
    }
}
