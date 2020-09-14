using Windows.Storage;

namespace ProjectCodeEditor.Models
{
    public sealed class RecentItem
    {
        public string DisplayName { get; set; }

        public string Location
        {
            get
            {
                if (IsWeb) return Token;
                else return FileHandle.Path;
            }
        }

        public bool IsWeb = false;

        public StorageFile FileHandle { get; set; }

        public string Token { get; set; }
    }
}
