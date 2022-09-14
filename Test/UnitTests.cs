using NBagOfTricks.PNUT;
using NBagOfTricks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
                for (int i = 0; i <= 90000000; i+= 1234567)
                {
                    float msec = Converters.SampleToMsec(i);
                    int sout = Converters.MsecToSample(msec);
                    int diff = Math.Abs(sout - i);
                    UT_LESS_OR_EQUAL(diff, 5); // Likely small conversion error. TODO revisit all these.
                }
            }

            // TimeSpan <-> sample.
            {
                int sample1 = 62398541;
                int sample2 = 1904788;

                var ts = Converters.SampleToTime(sample1, SnapType.None);
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "23:34.933");
                int sout = Converters.TimeToSample(ts);
                int diff = Math.Abs(sample1 - sout);
                UT_LESS(diff, 50); // Larger conversion error.

                ts = Converters.SampleToTime(sample2, SnapType.None);
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "00:43.192");
                sout = Converters.TimeToSample(ts);
                diff = Math.Abs(sample2 - sout);
                UT_LESS(diff, 50); // Larger conversion error.

                ts = Converters.SampleToTime(sample1, SnapType.Fine); // round down
                UT_EQUAL(ts.ToString(AudioLibDefs.TS_FORMAT), "23:34.900");//62397090

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

                float bpm = 76.58f;
                var bb = Converters.SampleToBarBeat(sample1, bpm, SnapType.None);
                UT_EQUAL(bb.ToString(), "580.4.02");
                int sout = Converters.BarBeatToSample(bb, bpm);
                int diff = Math.Abs(sample1 - sout);
                UT_LESS(diff, 100); // Larger conversion error.

                bpm = 117.02f;
                bb = Converters.SampleToBarBeat(sample2, bpm, SnapType.None);
                UT_EQUAL(bb.ToString(), "101.1.49");
                sout = Converters.BarBeatToSample(bb, bpm);
                diff = Math.Abs(sample2 - sout);
                UT_LESS(diff, 100); // Larger conversion error.

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
