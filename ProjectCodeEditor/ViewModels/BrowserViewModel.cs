using ProjectCodeEditor.Helpers;
using System.Collections;

namespace ProjectCodeEditor.ViewModels
{
    public class BrowserViewModel : Observable
    {
        private BitArray SmallStorage = new BitArray(3);

        public bool ProgressRingOn
        {
            get => SmallStorage.Get(0);
            set
            {
                SmallStorage.Set(0, value);
                OnPropertyChanged(nameof(ProgressRingOn));
            }
        }

        public bool ContentShown
        {
            get => SmallStorage.Get(1);
            set
            {
                SmallStorage.Set(1, value);
                OnPropertyChanged(nameof(ContentShown));
            }
        }

        public bool IsLoading
        {
            get => SmallStorage.Get(2);
            set
            {
                SmallStorage.Set(2, value);
                OnPropertyChanged(nameof(IsLoading));
            }
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
