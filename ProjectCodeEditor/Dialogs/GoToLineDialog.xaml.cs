using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Controls;

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
