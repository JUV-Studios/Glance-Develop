using ProjectCodeEditor.Helpers;
using System.Text;
using Windows.Graphics.Printing;
using Windows.Storage;

namespace ProjectCodeEditor.ViewModels
{
    public class MonacoEditorViewModel : Observable
    {
        private StorageFile _WorkingFile;

        public bool CanPrint = PrintManager.IsSupported();

        private Encoding _Encoding;

        public Encoding Encoding
        {
            get => _Encoding;
            set => Set(ref _Encoding, value);
        }

        public StorageFile WorkingFile
        {
            get => _WorkingFile;
            set => Set(ref _WorkingFile, value);
        }

        public MonacoEditorViewModel()
        {
        }
    }
}
