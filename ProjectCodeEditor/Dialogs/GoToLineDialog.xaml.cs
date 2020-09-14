using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class GoToLineDialog : ContentDialog
    {
        public event EventHandler<bool> Submitted;

        public async void Show(uint lines)
        {
            lineBox.Maximum = lines;
            lineBox.Minimum = 1;
            await ShowAsync();
        }

        public uint LineSelected
        {
            get => Convert.ToUInt32(lineBox.Value);
        }

        public GoToLineDialog()
        {
            InitializeComponent();
        }

        private void Cancel_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Submitted?.Invoke(this, false);
            Hide();
        }

        private void Go_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Submitted?.Invoke(this, true);
            Hide();
        }

        private void lineBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            sender.Value = Convert.ToUInt32(sender.Value);
        }
    }
}
