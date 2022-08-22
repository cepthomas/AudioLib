using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using NBagOfTricks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace AudioLib
{
    /// <summary>Simple mono wave display.</summary>
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Cascadia", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>The data buffer.</summary>
        float[] _vals = Array.Empty<float>();

        /// <summary>Maximum gain.</summary>
        const float MAX_GAIN = 5.0f;

        /// <summary>Grid Y resolution. Assumes +-1.0f range.</summary>
        const float GRID_STEP = 0.25f;
        #endregion

        #region Backing fields
        float _gain = 1.0f;
        readonly SolidBrush _brushSel = new(Color.White);
        readonly Pen _penDraw = new(Color.Black, 1);
        readonly Pen _penGrid = new(Color.LightGray, 1);
        #endregion

        #region Properties
        /// <summary>The waveform color.</summary>
        public Color DrawColor { get { return _penDraw.Color; } set { _penDraw.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color GridColor { get { return _penGrid.Color; } set { _penGrid.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color SelColor { get { return _brushSel.Color; } set { _brushSel.Color = value; } }

        /// <summary>Amplitude adjustment.</summary>
        public float Gain { get { return _gain; } set { _gain = value; Invalidate(); } }

        /// <summary>There isn't enough data to fill full width so disallow navigation.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Frozen { get; private set; } = false;

        /// <summary>Selection snaps to this increment value.</summary>
        public float SnapTODO { get; set; } = 0;

        /// <summary>Selection start.</summary>
        public int SelStart { get; set; } = -1;

        /// <summary>Selection length. Could be negative.</summary>
        public int SelLength { get; set; } = 0;

        /// <summary>Current cursor.</summary>
        public int ViewCursor { get; set; } = -1;

        /// <summary>Visible start sample.</summary>
        public int VisStart { get; set; } = 0;

        /// <summary>Visible length. Always positive.</summary>
        public int VisLength { get; set; } = 0;
        #endregion

        #region Events
        /// <summary>WaveViewer has something to say or show.</summary>
        public class StatusEventArgs : EventArgs
        {
            /// <summary>0 -> 100</summary>
            public float Gain { get; set; } = 0.0f;

            /// <summary>Some information.</summary>
            public string Message { get; set; } = "";
        }

        public event EventHandler<StatusEventArgs>? StatusEvent;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WaveViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Set everything from data source. Do this before setting properties by client.
        /// </summary>
        /// <param name="prov">Source</param>
        public void Init(ISampleProvider prov)
        {
            _vals = prov.ReadAll().vals;

            _gain = 1.0f;
            SelStart = -1;
            SelLength = 0;
            ViewCursor = -1;
            VisStart = 0;
            VisLength = _vals.Length;

            Invalidate();
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

        #region UI handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            if (ModifierKeys == Keys.Control) // x zoom TODO
            {
                if(!Frozen)
                {
                    Invalidate();
                }
            }
            else if (ModifierKeys == Keys.Shift) // y gain
            {
                _gain += hme.Delta > 0 ? 0.1f : -0.1f;
                _gain = (float)MathUtils.Constrain(_gain, 0.0f, MAX_GAIN);
                Invalidate();
            }
            else if (ModifierKeys == Keys.None) // no mods = x shift TODO
            {
                if (!Frozen)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Handle mouse wheel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:

                    if (ModifierKeys == Keys.None)
                    {
                        ViewCursor = GetSampleFromMouse();
                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Control)
                    {
                        SelStart = GetSampleFromMouse();
                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Shift && SelStart != -1)
                    {
                        var sel = GetSampleFromMouse();
                        SelLength = sel - SelStart;
                        Invalidate();
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle mouse move.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // if (e.Button == MouseButtons.Left)
            // {
            //     _current = GetTimeFromMouse(e.X);
            //     CurrentTimeChanged?.Invoke(this, new EventArgs());
            // }
            // else
            // {
            //     if (e.X != _lastXPos)
            //     {
            //         TimeSpan ts = GetTimeFromMouse(e.X);
            //         _toolTip.SetToolTip(this, ts.ToString(AudioLibDefs.TS_FORMAT));
            //         _lastXPos = e.X;
            //     }
            // }

            // Invalidate();
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Key press.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //case Keys.Escape:
                case Keys.G:
                    _gain = 1.0f;
                    Invalidate();
                    break;

                case Keys.H:
                    VisStart = 0;
                    VisLength = _vals.Length;
                    Invalidate();
                    break;
            }
        }

        /// <summary>
        /// Resize.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Paint the waveform.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Setup.
            pe.Graphics.Clear(BackColor);

            ValidateProperties(); // good time to do this

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
            }
            else
            {
                // First selection area.
                if(SelStart != -1 && SelLength > 0)
                {

                    for (int i = 0; i < SelLength; i++)
                    {

                    }
                }

                // Then the grid lines.
                _penGrid.Width = 1;
                for (float gs = -5 * GRID_STEP; gs <= 5 * GRID_STEP; gs += GRID_STEP)
                {
                    float yGrid = MathUtils.Map(gs, 1.0f, -1.0f, 0, Height);
                    pe.Graphics.DrawLine(_penGrid, 0, yGrid, Width, yGrid);
                }

                // Y zero is a bit thicker.
                _penGrid.Width = 3;
                float yZero = MathUtils.Map(0.0f, 1.0f, -1.0f, 0, Height);
                pe.Graphics.DrawLine(_penGrid, 0, yZero, Width, yZero);

                // Then the data.
                if(!Frozen)
                {
                    int samplesPerPixel = VisLength / Width;
                    var peaks = PeakProvider.GetPeaks(_vals, VisStart, samplesPerPixel, Width);
                    for (int i = 0; i < peaks.Count; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yMax = (int)MathUtils.Map(peaks[i].max * _gain, 1.0f, -1.0f, 0, Height);
                        int yMin = (int)MathUtils.Map(peaks[i].min * _gain, 1.0f, -1.0f, 0, Height);

                        // Make sure there's at least one dot.
                        if (yMax == yMin)
                        {
                            if (yMax > 0) { yMin--; }
                            else { yMax++; }
                        }

                        pe.Graphics.DrawLine(_penDraw, i, yMax, i, yMin);
                    }
                }
                else
                {
                    // Not enough data - just show what we have.
                    for (int i = 0; i < _vals.Length; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yVal = (int)MathUtils.Map(_vals[i] * _gain, 1.0f, -1.0f, 0, Height);
                        pe.Graphics.DrawRectangle(_penDraw, i, yVal, 1, 1);
                    }
                }

                // Then cursor.
                if (ViewCursor != -1)
                {
                    int x = MathUtils.Map(ViewCursor, 0, _vals.Length, 0, Width);
                    pe.Graphics.DrawLine(_penDraw, x, 0, x, Height);
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Check sanity of client selections.
        /// </summary>
        void ValidateProperties()
        {
            SelStart = MathUtils.Constrain(SelStart, -1, _vals.Length);
            SelLength = MathUtils.Constrain(SelLength, -_vals.Length, _vals.Length);

            ViewCursor = MathUtils.Constrain(ViewCursor, -1, _vals.Length);

            VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length);
            if (VisLength == 0) { VisLength = _vals.Length; }
            VisLength = MathUtils.Constrain(VisLength, 0, _vals.Length - VisStart);

            int samplesPerPixel = VisLength / Width;
            Frozen = samplesPerPixel == 0;
        }

        /// <summary>
        /// Convert x pos to sample index.
        /// </summary>
        /// <param name="x">UI loc or -1 if get current.</param>
        int GetSampleFromMouse(int x = -1)
        {
            if(x < 0)
            {
                x = PointToClient(Cursor.Position).X;
            }
            int sample = MathUtils.Map(x, 0, Width, VisStart, VisStart + VisLength);
            return sample;
        }

        /// <summary>
        /// Snap to user preference.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        int DoSnap(int sample) // TODO
        {
            //int smsec = 0;

            //if (SnapMsec > 0)
            //{
            //    smsec = (msec / SnapMsec) * SnapMsec;
            //    if (SnapMsec > (msec % SnapMsec) / 2)
            //    {
            //        smsec += SnapMsec;
            //    }
            //}

            return sample;
        }
        #endregion
    }
}
