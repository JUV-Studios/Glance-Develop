using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ProjectCodeEditor.Models
{
    public sealed class ActionOption : ObservableObject
    {
        private string _Title;
        private string _Description;
        private IconElement _Icon;
        private Action _Command;
        private string _AccessKey;
        private bool _IsEnabled = true;

        public string Title
        {
            get => _Title;
            set
            {
                SetProperty(ref _Title, value);
                OnPropertyChanged(nameof(ToolTipContent));
            }
        }

        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        public IconElement Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        public Action Command
        {
            get => _Command;
            set => _Command = value;
        }

        public event EventHandler Click;

        internal void RaiseClick() => Click?.Invoke(this, null);

        public string AccessKey
        {
            get => _AccessKey;
            set
            {
                SetProperty(ref _AccessKey, value);
                OnPropertyChanged(nameof(ToolTipContent));
            }
        }

        public bool IsEnabled
        {
            get => _IsEnabled;
            set => SetProperty(ref _IsEnabled, value);
        }

        public XamlUICommand CommandRef
        {
            set
            {
                Title = value.Label;
                Description = value.Description;
                Icon = new IconSourceElement() { IconSource = value.IconSource };
                if (!string.IsNullOrWhiteSpace(value.AccessKey)) AccessKey = value.AccessKey;
            }
        }

        public string ToolTipContent
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AccessKey)) return Title;
                else return $"{Title} ({AccessKey})";
            }
        }

        public override string ToString() => $"{Title}, {Description}";
    }
}
