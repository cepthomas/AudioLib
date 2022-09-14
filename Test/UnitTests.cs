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
    public class CONVERTERS : TestSuite
    {
        public override void RunSuite()
        {
            // Sample <-> sample.
            {
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

            // Milliseconds <-> sample.
            {

                int errmax = 5; // Roundtrip conversion error.
                for (int i = 0; i <= 90000000; i+= 1234567)
                {
                    float msec = Converters.SampleToMsec(i);
                    int sout = Converters.MsecToSample(msec);
                    int err = Math.Abs(sout - i);
                    UT_LESS_OR_EQUAL(err, errmax);
                }
            }

            // TimeSpan <-> sample.
            {
                int sample1 = 62398541;
                int sample2 = 1904788;
                int errmax = 50; // Roundtrip conversion error.

                var ts = Converters.SampleToTime(sample1, SnapType.None);
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "23:34.933");
                int sout = Converters.TimeToSample(ts);
                int err = Math.Abs(sample1 - sout); //4
                Debug.WriteLine($"TimeSpan error:{Converters.SampleToMsec(err)}");// TimeSpan error:0.09070295
                UT_LESS(err, errmax);

                ts = Converters.SampleToTime(sample2, SnapType.None);
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "00:43.192");
                sout = Converters.TimeToSample(ts);
                err = Math.Abs(sample2 - sout);//21
                Debug.WriteLine($"TimeSpan error:{Converters.SampleToMsec(err)}");// TimeSpan error:0.47619048
                UT_LESS(err, errmax);

                ts = Converters.SampleToTime(sample1, SnapType.Fine); // round down
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "23:34.900");

                ts = Converters.SampleToTime(sample2, SnapType.Fine); // round up
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "00:43.200");

                ts = Converters.SampleToTime(sample1, SnapType.Coarse); // round up
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "23:35.000");

                ts = Converters.SampleToTime(sample2, SnapType.Coarse); // round down
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "00:43.000");
            }

            // BarBeat <-> sample.
            {
                int sample1 = 80126934;
                int sample2 = 9055612;
                int errmax = 100; // Roundtrip conversion error.

                float bpm = 76.58f;
                var bb = Converters.SampleToBarBeat(sample1, bpm, SnapType.None);
                UT_EQUAL(bb.ToString(), "580.4.02");
                int sout = Converters.BarBeatToSample(bb, bpm);
                int err = Math.Abs(sample1 - sout);
                Debug.WriteLine($"BarBeat error:{Converters.SampleToMsec(err)}");// BarBeat error:1.8594104
                UT_LESS(err, errmax);

                bpm = 117.02f;
                bb = Converters.SampleToBarBeat(sample2, bpm, SnapType.None);
                UT_EQUAL(bb.ToString(), "101.1.49");
                sout = Converters.BarBeatToSample(bb, bpm);
                err = Math.Abs(sample2 - sout);
                Debug.WriteLine($"BarBeat error:{Converters.SampleToMsec(err)}");// BarBeat error:1.723356
                UT_LESS(err, errmax);

                bb = new() { Bar = 421, Beat = 2, PartBeat = 49 }; // round beat down
                var bbout = Converters.SnapBarBeat(bb, SnapType.Fine);
                UT_EQUAL(bbout.Bar, 421);
                UT_EQUAL(bbout.Beat, 2);
                UT_EQUAL(bbout.PartBeat, 0);

                bb = new() { Bar = 421, Beat = 1, PartBeat = 51 }; // round beat up
                bbout = Converters.SnapBarBeat(bb, SnapType.Fine);
                UT_EQUAL(bbout.Bar, 421);
                UT_EQUAL(bbout.Beat, 2);
                UT_EQUAL(bbout.PartBeat, 0);

                bb = new() { Bar = 421, Beat = 3, PartBeat = 99 }; // round beat up
                bbout = Converters.SnapBarBeat(bb, SnapType.Fine);
                UT_EQUAL(bbout.Bar, 422);
                UT_EQUAL(bbout.Beat, 0);
                UT_EQUAL(bbout.PartBeat, 0);

                bb = new() { Bar = 421, Beat = 3, PartBeat = 88 }; // round bar up
                bbout = Converters.SnapBarBeat(bb, SnapType.Coarse);
                UT_EQUAL(bbout.Bar, 422);
                UT_EQUAL(bbout.Beat, 0);
                UT_EQUAL(bbout.PartBeat, 0);

                bb = new() { Bar = 421, Beat = 1, PartBeat = 48 }; // round bar down
                bbout = Converters.SnapBarBeat(bb, SnapType.Coarse);
                UT_EQUAL(bbout.Bar, 421);
                UT_EQUAL(bbout.Beat, 0);
                UT_EQUAL(bbout.PartBeat, 0);
            }
        }
    }
}
