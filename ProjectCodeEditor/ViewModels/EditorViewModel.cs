using ProjectCodeEditor.Helpers;
using System.Text;
using Windows.Graphics.Printing;
using Windows.Storage;

namespace ProjectCodeEditor.ViewModels
{
    public class EditorViewModel : Observable
    {
        private StorageFile _WorkingFile;
        private string _WorkString;
        public bool CanPrint = PrintManager.IsSupported();
        private Encoding _Encoding;

        public Encoding Encoding
        {
            get => _Encoding;
            set => Set(ref _Encoding, value);
        }

        public string WorkString
        {
            get => _WorkString;
            set => Set(ref _WorkString, value);
        }

        public StorageFile WorkingFile
        {
            get => _WorkingFile;
            set => Set(ref _WorkingFile, value);
        }

        public EditorViewModel()
        {
        }
    }
}
