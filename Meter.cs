using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>Display types.</summary>
    public enum MeterType { Linear, Log, ContinuousLine, ContinuousDots }  // FUTURE Add damped/vu, peak/hold

    /// <summary>
    /// Implements a rudimentary volume meter.
    /// </summary>
    public partial class Meter : UserControl
    {
        #region Fields
        /// <summary>Storage.</summary>
        double[] _buff = Array.Empty<double>();

        /// <summary>Storage.</summary>
        int _buffIndex = 0;

        /// <summary>The pen.</summary>
        readonly Pen _pen = new(Color.Black, 1);

        /// <summary>The brush.</summary>
        readonly SolidBrush _brush = new(Color.White);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        #endregion

        #region Properties
        /// <summary>Optional label.</summary>
        public string Label { get; set; } = "";

        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _brush.Color; } set { _brush.Color = value; } }

        /// <summary>How the meter responds.</summary>
        public MeterType MeterType { get; set; } = MeterType.Linear;

        /// <summary>Minimum value. If Log type, this is in db - usually -60;</summary>
        public double Minimum { get; set; } = 0;

        /// <summary>Maximum value. If Log type, this is in db - usually +18.</summary>
        public double Maximum { get; set; } = 100;

        /// <summary>Meter orientation.</summary>
        public Orientation Orientation { get; set; } = Orientation.Horizontal;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Basic volume meter.
        /// </summary>
        public Meter()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Name = "Meter";
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
                _brush.Dispose();
                _format.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Add a new data point. If Log, this will convert for you.
        /// </summary>
        /// <param name="val"></param>
        public void AddValue(double val)
        {
            // Sometimes when you minimize, samples can be set to 0.
            if (_buff.Length != 0)
            {
                switch (MeterType)
                {
                    case MeterType.Log:
                        double db = MathUtils.Constrain(20 * Math.Log10(val), Minimum, Maximum);
                        _buff[0] = db;
                        break;

                    case MeterType.Linear:
                        double lval = MathUtils.Constrain(val, Minimum, Maximum);
                        _buff[0] = lval;
                        break;

                    case MeterType.ContinuousLine:
                    case MeterType.ContinuousDots:
                        // Bump ring index.
                        _buffIndex++;
                        _buffIndex %= _buff.Length;
                        _buff[_buffIndex] = MathUtils.Constrain(val, Minimum, Maximum);
                        break;
                }

                Invalidate();
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Paints the volume meter.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.Clear(BackColor);

            // Draw data.
            switch (MeterType)
            {
                case MeterType.Log:
                case MeterType.Linear:
                    double percent = _buff.Length > 0 ? (_buff[0] - Minimum) / (Maximum - Minimum) : 0;

                    if (Orientation == Orientation.Horizontal)
                    {
                        int w = (int)(ClientRectangle.Width * percent);
                        int h = ClientRectangle.Height;
                        pe.Graphics.FillRectangle(_brush, 0, 0, w, h);
                    }
                    else
                    {
                        int w = ClientRectangle.Width;
                        int h = (int)(ClientRectangle.Height * percent);
                        pe.Graphics.FillRectangle(_brush, 0, Height - h, w, h);
                    }
                    break;

                case MeterType.ContinuousLine:
                case MeterType.ContinuousDots:
                    for (int i = 0; i < _buff.Length; i++)
                    {
                        int index = _buffIndex - i;
                        index = index < 0 ? index + _buff.Length : index;

                        double val = _buff[index];

                        // Draw data point.
                        double y = MathUtils.Map(val, Minimum, Maximum, ClientRectangle.Height, 0);

                        if (MeterType == MeterType.ContinuousLine)
                        {
                            pe.Graphics.DrawLine(_pen, (float)i, (float)y, (float)i, ClientRectangle.Height);
                        }
                        else
                        {
                            pe.Graphics.FillRectangle(_brush, (float)i, (float)y, 2, 2);
                        }
                    }
                    break;
            }

            if (Label.Length > 0)
            {
                Rectangle r = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height / 2);
                pe.Graphics.DrawString(Label, Font, Brushes.Black, r, _format);
            }
        }

        /// <summary>
        /// Update drawing area.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            int buffSize = Width;
            _buff = new double[buffSize];
            for(int i = 0; i < buffSize; i++)
            {
                _buff[i] = Minimum;
            }
            _buffIndex = 0;
            base.OnResize(e);
            Invalidate();
        }
        #endregion
    }
}
