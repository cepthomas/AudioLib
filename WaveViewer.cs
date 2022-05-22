using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NBagOfTricks;


namespace AudioLib
{
    public partial class WaveViewer : UserControl
    {
        public enum DrawMode { Raw, Envelope }

        #region Fields
        /// <summary>From client.</summary>
        float[]? _rawVals = null;

        /// <summary>Maximum value of _rawVals (+-).</summary>
        float _rawMax = 1.0f;

        /// <summary>Storage for display.</summary>
        float[]? _scaledBuff = null;

        /// <summary>For drawing.</summary>
        readonly Pen _penDraw = new(Color.Black, 1);

        /// <summary>For drawing.</summary>
        readonly Pen _penMarker = new(Color.Black, 1);

        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Cascadia", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>Ratio of data point to visual point.</summary>
        int _smplPerPixel;
        #endregion

        int _marker1 = -1;
        int _marker2 = -1;

        #region Properties
        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _penDraw.Color; } set { _penDraw.Color = value; } }

        /// <summary>For styling.</summary>
        public Color MarkerColor { get { return _penMarker.Color; } set { _penMarker.Color = value; } }

        /// <summary>How to draw.</summary>
        public DrawMode Mode { get; set; } = DrawMode.Envelope;

        /// <summary>Marker 1 data index or -1 to disable.</summary>//TODOX do something with these + show progress. Make into array.
        public int Marker1
        {
            get
            {
                return _marker1;
            }
            set
            {
                if (value < 0 || _rawVals is null)
                {
                    _marker1 = -1;
                }
                else
                {
                    _marker1 = MathUtils.Constrain(value, 0, _rawVals.Length);
                }
                Invalidate();
            }
        }

        /// <summary>Marker 2 data index or -1 to disable.</summary>
        public int Marker2
        {
            get
            {
                return _marker2;
            }
            set
            {
                if (value < 0 || _rawVals is null)
                {
                    _marker2 = -1;
                }
                else
                {
                    _marker2 = MathUtils.Constrain(value, 0, _rawVals.Length);
                }
                Invalidate();
            }
        }
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
                _penDraw.Dispose();
                _textFont.Dispose();
                _format.Dispose();
           }
           base.Dispose(disposing);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Populate with data.
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="max"></param>
        public void Init(float[]? vals, float max)
        {
            //Dump(vals, "raw.csv");
            _rawVals = vals;
            _rawMax = max;
            _marker1 = -1;
            _marker2 = -1;

            Rescale();
            //Dump(_buff, "buff.csv");
            Invalidate();
        }

        /// <summary>
        /// Hard reset.
        /// </summary>
        public void Reset()
        {
            _rawVals = null;
            _scaledBuff = null;
            _rawMax = 0;
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

            if (_scaledBuff is null)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
            }
            else
            {
                // Draw data points.
                for (int i = 0; i < _scaledBuff.Length; i++)
                {
                    float val = _scaledBuff[i];

                    if(!float.IsNaN(val))
                    {
                        switch (Mode)
                        {
                            case DrawMode.Envelope:
                                float y1 = (float)MathUtils.Map(val, -_rawMax, _rawMax, Height, 0);
                                //float y2 = Height / 2; // Line from val to 0
                                float y2 = (float)MathUtils.Map(val, -_rawMax, _rawMax, 0, Height); // Line from +val to -val
                                pe.Graphics.DrawLine(_penDraw, i, y1, i, y2);
                                break;

                            case DrawMode.Raw:
                                // Simple dot
                                float y = (float)MathUtils.Map(val, -_rawMax, _rawMax, Height, 0);
                                pe.Graphics.DrawRectangle(_penDraw, i, y, 1, 1);
                                break;
                        }
                    }
                }

                // Draw  markers.
                if (_marker1 > 0)
                {
                    int x = _smplPerPixel > 0 ? _marker1 / _smplPerPixel : _marker1;
                    pe.Graphics.DrawLine(_penMarker, x, 0, x, Height);
                }

                if (_marker2 > 0)
                {
                    int x = _smplPerPixel > 0 ? _marker2 / _smplPerPixel : _marker2;
                    pe.Graphics.DrawLine(_penMarker, x, 0, x, Height);
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Scale raw values to fit in available space.
        /// </summary>
        void Rescale()
        {
            if(_rawVals is null)
            {
                _scaledBuff = null;
            }
            else
            {
                int fitWidth = Width;
                _smplPerPixel = _rawVals.Length / fitWidth;
                _scaledBuff = new float[fitWidth];

                if (_smplPerPixel > 0)
                {
                    int r = 0; // index into raw
                    for (int i = 0; i < fitWidth; i++)
                    {
                        float[] subset = new float[_smplPerPixel];
                        Array.Copy(_rawVals, r, subset, 0, _smplPerPixel);
                        var rms = MathUtils.RMS(subset);
                        _scaledBuff[i] = rms;
                        r += _smplPerPixel;
                    }
                }
                else
                {
                    for (int i = 0; i < _scaledBuff.Length; i++)
                    {
                        _scaledBuff[i] = i < _rawVals.Length ? _rawVals[i] : float.NaN;
                    }
                }
            }
        }

        /// <summary>
        /// Update drawing area.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Rescale();
            Invalidate();
        }

        /// <summary>
        /// Simple utility.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fn"></param>
        void Dump(float[] data, string fn)
        {
            if (data is not null)
            {
                List<string> ss = new List<string>();
                for (int i = 0; i < data.Length; i++)
                {
                    ss.Add($"{i + 1}, {data[i]}");
                }
                File.WriteAllLines(fn, ss);
            }
        }
        #endregion
    }
}
