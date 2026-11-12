using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using NAudio.Wave;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    /// <summary>Select one from available output devices.</summary>
    public class WaveOutputDeviceTypeEditor : UITypeEditor
    {
        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            if (provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService _service || context is null || context.Instance is null) { return null; }

            var lb = new ListBox(); // {Width = 50,SelectionMode = SelectionMode.One};

            List<string> opts = [];
            for (int id = -1; id < WaveOut.DeviceCount; id++) // –1 indicates the default output device, while 0 is the first output device.
            {
                var cap = WaveOut.GetCapabilities(id);
                opts.Add(cap.ProductName);
            }

            opts.ForEach(v => lb.Items.Add(v));
            lb.Click += (_, __) => _service.CloseDropDown();
            _service.DropDownControl(lb);

            return lb.SelectedItem is null ? value : lb.SelectedItem; //???
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) { return UITypeEditorEditStyle.DropDown; }
    }


    /// <summary>Select one from available values.</summary>
    public class LatencyTypeEditor : UITypeEditor
    {
        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            if (provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService _service || context is null || context.Instance is null) { return null; }

            var lb = new ListBox(); // {Width = 50,SelectionMode = SelectionMode.One};
            List<string> opts = [ "25", "50", "100", "150", "200", "300", "400", "500" ];

            opts.ForEach(v => lb.Items.Add(v.ToString()));
            lb.Click += (_, __) => _service.CloseDropDown();
            _service.DropDownControl(lb);

            return lb.SelectedItem is null ? value : int.Parse((string)lb.SelectedItem);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) { return UITypeEditorEditStyle.DropDown; }
    }
}
