using ProjectCodeEditor.Helpers;

namespace ProjectCodeEditor.ViewModels
{
    public class BrowserViewModel : Observable
    {
        private bool _ProgressRingOn = false;

        public bool ProgressRingOn
        {
            get => _ProgressRingOn;
            set => Set(ref _ProgressRingOn, value);
        }

        private string _WorkString;

        public string WorkString
        {
            get => _WorkString;
            set => Set(ref _WorkString, value);
        }

        public BrowserViewModel()
        {
        }
    }
}
