using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NBagOfTricks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioLib
{
    /// <summary>Simple stereo/mono wave display.</summary>
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>For drawing.</summary>
        readonly Pen _pen = new(Color.Black, 1);

        /// <summary>For drawing.</summary>
        readonly Pen _penMarker = new(Color.Black, 2);

        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Cascadia", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>The main buffer from client - mono or left.</summary>
        float[] _vals = Array.Empty<float>();

        /// <summary>The secondary buffer from client - right.</summary>
        float[] _vals2 = Array.Empty<float>();
        #endregion


        ///// <summary>
        ///// Show this data.
        ///// </summary>
        ///// <param name="vals">Required mono or left channel data.</param>
        ///// <param name="vals2">Optional right channel.</param>
        //public void SetValues(float[] vals, float[]? vals2 = null)
        //{
        //    _vals = vals;
        //    _vals2 = vals2 ?? Array.Empty<float>();
        //    Invalidate();
        //}




        public ISampleProvider SampleProvider { set { ReadValues(value); } }



        #region Properties
        ///// <summary>The full buffer from client.</summary>
       // public float[] Values { get { return _vals; } set { _vals = value; Invalidate(); } }
        //float[] _vals = Array.Empty<float>();

        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _pen.Color; } set { _pen.Color = value; Invalidate(); } }

        /// <summary>Marker index.</summary>
        public int Marker { get { return _marker; } set { _marker = value; Invalidate(); } }
        int _marker = -1;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
           if (disposing)
           {
                _pen.Dispose();
                _textFont.Dispose();
                _format.Dispose();
           }
           base.Dispose(disposing);
        }

        /// <summary>
        /// Hard reset.
        /// </summary>
        public void Reset()
        {
            _vals = Array.Empty<float>();
            _marker = -1;
            Invalidate();
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Paints the waveform.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Setup.
            pe.Graphics.Clear(BackColor);

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
                return; // >>> early
            }

            bool stereo = _vals2 is not null && _vals2.Length > 0;

            int border = 5;
            int fitWidth = Width - (2 * border);
            int fitHeight = Height - (2 * border);
            int numVals = _vals.Length;
            int samplesPerPixel = _vals.Length / fitWidth;

            // Left/mono.
            var peaks = PeakProvider.GetPeaks(_vals, 0, samplesPerPixel, fitWidth);
            float offset = stereo ? 0.5f : 1.0f;
            float mult = stereo ? 0.25f : 0.5f;
            DrawOneWave();

            // Right.
            if(stereo)
            {
                offset = 1.5f;
                peaks = PeakProvider.GetPeaks(_vals2!, 0, samplesPerPixel, fitWidth);
                DrawOneWave();
            }

            // Local common func.
            void DrawOneWave()
            {
                for (int i = 0; i < peaks.Count; i++)
                {
                    float yMax = border + fitHeight - ((peaks[i].max + offset) * mult * fitHeight);
                    float yMin = border + fitHeight - ((peaks[i].min + offset) * mult * fitHeight);
                    pe.Graphics.DrawLine(_pen, i + border, yMax, i + border, yMin);
                }
            }
        }
        #endregion

        #region Private
        /// <summary>
        /// Read mono or stereo from sample provider.
        /// </summary>
        /// <param name="source"></param>
        void ReadValues(ISampleProvider source)
        {
            //var vvv = AudioUtils.ReadAll(source);

            if (source.WaveFormat.Channels == 2)
            {
                AudioUtils.SetProviderPosition(source, 0);
                var valsL = new StereoToMonoSampleProvider(source) { LeftVolume = 1.0f, RightVolume = 0.0f };
                _vals = AudioUtils.ReadAll(valsL);

                AudioUtils.SetProviderPosition(source, 0);
                var valsR = new StereoToMonoSampleProvider(source) { LeftVolume = 0.0f, RightVolume = 1.0f };
                _vals2 = AudioUtils.ReadAll(valsR);
            }
            else
            {
                _vals = AudioUtils.ReadAll(source);
            }

            Invalidate();
        }
        #endregion
    }

    ///// <summary>
    ///// Takes a stereo input and turns it to mono
    ///// </summary>
    //public class StereoToMonoSampleProvider_XXX : ISampleProvider
    //{
    //    private readonly ISampleProvider _sourceProvider;

    //    private float[] _sourceBuffer;

    //    /// <summary>
    //    /// Creates a new mono ISampleProvider based on a stereo input
    //    /// </summary>
    //    /// <param name="sourceProvider">Stereo 16 bit PCM input</param>
    //    public StereoToMonoSampleProvider_XXX(ISampleProvider sourceProvider)
    //    {
    //        LeftVolume = 0.5f;
    //        RightVolume = 0.5f;

    //        AudioUtils.ValidateFormat(sourceProvider.WaveFormat, false);

    //        if (sourceProvider.WaveFormat.Channels != 2) // TODO combine.
    //        {
    //            throw new ArgumentException("Source must be stereo");
    //        }

    //        _sourceProvider = sourceProvider;
    //        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sourceProvider.WaveFormat.SampleRate, 1);
    //    }

    //    /// <summary>
    //    /// 1.0 to mix the mono source entirely to the left channel
    //    /// </summary>
    //    public float LeftVolume { get; set; }

    //    /// <summary>
    //    /// 1.0 to mix the mono source entirely to the right channel
    //    /// </summary>
    //    public float RightVolume { get; set; }

    //    /// <summary>
    //    /// Output Wave Format
    //    /// </summary>
    //    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

    //    /// <summary>
    //    /// Reads floats from this SampleProvider
    //    /// </summary>
    //    public int Read(float[] buffer, int offset, int count)
    //    {
    //        var sourceSamplesRequired = count * 2;

    //        if (_sourceBuffer == null || _sourceBuffer.Length < sourceSamplesRequired)
    //        {
    //            _sourceBuffer = new float[sourceSamplesRequired];
    //        }

    //        var sourceSamplesRead = _sourceProvider.Read(_sourceBuffer, 0, sourceSamplesRequired);
    //        var destOffset = offset;

    //        for (var sourceSample = 0; sourceSample < sourceSamplesRead; sourceSample += 2)
    //        {
    //            var left = _sourceBuffer[sourceSample];
    //            var right = _sourceBuffer[sourceSample + 1];
    //            var outSample = (left * LeftVolume) + (right * RightVolume);

    //            buffer[destOffset++] = outSample;
    //        }
    //        return sourceSamplesRead / 2;

    //        //var waveBuffer = new WaveBuffer(buffer);
    //        //int samplesRequired = count / 4;
    //        //int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
    //        //return samplesRead * 4;

    //    }


    //    public int Read_old(float[] buffer, int offset, int count)
    //    {
    //        var sourceSamplesRequired = count * 2;

    //        if (_sourceBuffer == null || _sourceBuffer.Length < sourceSamplesRequired)
    //        {
    //            _sourceBuffer = new float[sourceSamplesRequired];
    //        }

    //        var sourceSamplesRead = _sourceProvider.Read(_sourceBuffer, 0, sourceSamplesRequired);
    //        var destOffset = offset;

    //        for (var sourceSample = 0; sourceSample < sourceSamplesRead; sourceSample += 2)
    //        {
    //            var left = _sourceBuffer[sourceSample];
    //            var right = _sourceBuffer[sourceSample + 1];
    //            var outSample = (left * LeftVolume) + (right * RightVolume);

    //            buffer[destOffset++] = outSample;
    //        }
    //        return sourceSamplesRead / 2;
    //    }

    //}
}
