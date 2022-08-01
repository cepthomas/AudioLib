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
        #region Fields
        /// <summary>From client.</summary>
        float[]? _rawVals = null;

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

        /// <summary>Marker 1 data index or -1 to disable.</summary>
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
            _marker1 = -1;
            _marker2 = -1;
            Invalidate();
        }

        /// <summary>
        /// Hard reset.
        /// </summary>
        public void Reset()
        {
            _rawVals = null;
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

            if (_rawVals is null || _rawVals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
            }
            else
            {
                // https://stackoverflow.com/a/1215472
                int border = 5;
                float fitWidth = Width - (2 * border);
                float fitHeight = Height - (2 * border);
                float size = _rawVals.Length;


                float zoom = 0.01f;
                size *= zoom;

                for (int index = 0; index < fitWidth; index++)
                {
                    // determine start and end points within WAV
                    float start = index * (size / fitWidth);
                    float end = (index + 1) * (size / fitWidth);
                    float min = float.MaxValue;
                    float max = float.MinValue;
                    for (int i = (int)start; i < end; i++)
                    {
                        float val = _rawVals[i];
                        min = val < min ? val : min;
                        max = val > max ? val : max;
                    }
                    float yMax = border + fitHeight - ((max + 1) * 0.5f * fitHeight);
                    float yMin = border + fitHeight - ((min + 1) * 0.5f * fitHeight);
                    pe.Graphics.DrawLine(_penDraw, index + border, yMax, index + border, yMin);
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

        /// <summary>
        /// Update drawing area.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Simple utility.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fn"></param>
        void Dump(float[] data, string fn)
        {
            if (data is not null)
            {
                List<string> ss = new();
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
