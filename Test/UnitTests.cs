using NBagOfTricks.PNUT;
using NBagOfTricks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


//// Milliseconds <-> sample.
//{

//    int errmax = 5; // Roundtrip conversion error.
//    for (int i = 0; i <= 90000000; i+= 1234567)
//    {
//        float msec = i;
//        int sout = Converters.MsecToSample(msec);
//        int err = Math.Abs(sout - i);
//        UT_LESS_OR_EQUAL(err, errmax);
//    }
//}


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
            int sample1 = 62398541;
            int sample2 = 1904788;
            int errmax = 50; // Roundtrip conversion error.

            var tm = new AudioTime(sample1);
            UT_EQUAL(tm.ToString(), "23.34.933");
            int sout = tm.ToSample();
            int err = Math.Abs(sample1 - sout); //4
            Debug.WriteLine($"AudioTime error:{err}");// 0.09070295
            UT_LESS(err, errmax);

            tm = new AudioTime(sample2);
            UT_EQUAL(tm.ToString(), "00.43.192");
            sout = tm.ToSample();
            err = Math.Abs(sample2 - sout);//21
            Debug.WriteLine($"AudioTime error:{err}");// 0.47619048
            UT_LESS(err, errmax);

            tm = new AudioTime(sample1); // round down
            tm.Snap(SnapType.Fine);
            UT_EQUAL(tm.ToString(), "23.34.900");

            tm = new AudioTime(sample2); // round up
            tm.Snap(SnapType.Fine);
            UT_EQUAL(tm.ToString(), "00.43.200");

            tm = new AudioTime(sample1); // round up
            tm.Snap(SnapType.Coarse);
            UT_EQUAL(tm.ToString(), "23.35.000");

            tm = new AudioTime(sample2); // round down
            tm.Snap(SnapType.Coarse);
            UT_EQUAL(tm.ToString(), "00.43.000");

            // Parsing.
            // Good one.
            tm = AudioTime.Parse("32.47.123");
            UT_NOT_NULL(tm);
            UT_EQUAL(tm.Minutes, 32);
            UT_EQUAL(tm.Seconds, 47);
            UT_EQUAL(tm.Milliseconds, 123);
            UT_EQUAL(tm.TotalMilliseconds, 32 * 60 * 1000 + 47 * 1000 + 123);
            
            // Bad ones.
            tm = AudioTime.Parse("1:2");
            UT_NULL(tm);
            tm = AudioTime.Parse("1.2");
            UT_NULL(tm);
            tm = AudioTime.Parse("1.2.9999");
            UT_NULL(tm);
            tm = AudioTime.Parse("dsdsd");
            UT_NULL(tm);
        }
    }

    public class CONVERT_BARBEAT : TestSuite
    {
        public override void RunSuite()
        {
            // BarBeat <-> sample.
            int sample1 = 80126934;
            int sample2 = 9055612;
            int errmax = 100; // Roundtrip conversion error.

            float bpm = 76.58f;
            var bb = new BarBeat(sample1, bpm);
            UT_EQUAL(bb.ToString(), "579.3.02");
            int sout = bb.ToSample(bpm);
            int err = Math.Abs(sample1 - sout);
            Debug.WriteLine($"BarBeat error:{(err)}");// 1.8594104
            UT_LESS(err, errmax);

            bpm = 117.02f;
            bb = new BarBeat(sample2, bpm);
            UT_EQUAL(bb.ToString(), "100.0.49");
            sout = bb.ToSample(bpm);
            err = Math.Abs(sample2 - sout);
            Debug.WriteLine($"BarBeat error:{(err)}");// 1.723356
            UT_LESS(err, errmax);

            bb = new(421, 2, 49); // round beat down
            bb.Snap(SnapType.Fine);
            UT_EQUAL(bb.Bar, 421);
            UT_EQUAL(bb.Beat, 2);
            UT_EQUAL(bb.Subdiv, 0);

            bb = new(421, 1, 51); // round beat up
            bb.Snap(SnapType.Fine);
            UT_EQUAL(bb.Bar, 421);
            UT_EQUAL(bb.Beat, 2);
            UT_EQUAL(bb.Subdiv, 0);

            bb = new(421, 3, 99); // round beat up
            bb.Snap(SnapType.Fine);
            UT_EQUAL(bb.Bar, 422);
            UT_EQUAL(bb.Beat, 0);
            UT_EQUAL(bb.Subdiv, 0);

            bb = new(421, 3, 88); // round bar up
            bb.Snap(SnapType.Coarse);
            UT_EQUAL(bb.Bar, 422);
            UT_EQUAL(bb.Beat, 0);
            UT_EQUAL(bb.Subdiv, 0);

            bb = new(421, 1, 48); // round bar down
            bb.Snap(SnapType.Coarse);
            UT_EQUAL(bb.Bar, 421);
            UT_EQUAL(bb.Beat, 0);
            UT_EQUAL(bb.Subdiv, 0);

            // Parsing.
            // Good one.
            bb = BarBeat.Parse("32.2.78");
            UT_NOT_NULL(bb);
            UT_EQUAL(bb.Bar, 32);
            UT_EQUAL(bb.Beat, 2);
            UT_EQUAL(bb.Subdiv, 78);
            UT_EQUAL(bb.TotalSubdivs, 32 * 4 * 100 + 2 * 100 + 78);

            // Bad ones.
            bb = BarBeat.Parse("1:2");
            UT_NULL(bb);
            bb = BarBeat.Parse("1.2");
            UT_NULL(bb);
            bb = BarBeat.Parse("1.2.9999");
            UT_NULL(bb);
            bb = BarBeat.Parse("dsdsd");
            UT_NULL(bb);
        }
    }
}
