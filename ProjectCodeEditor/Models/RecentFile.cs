using Humanizer;
using Microsoft.Toolkit.Uwp.Extensions;
using System;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace ProjectCodeEditor.Models
{
    public sealed record RecentFile(StorageFile File, DateTime Time, AccessListEntry Entry)
    {
        private string _TimeString = null;
        public string TimeString
        {
            get
            {
                if (_TimeString == null) _TimeString = Time.Humanize(false);
                return _TimeString;
            }
        }

        public override string ToString() => $"{File.Name}, {File.Path}, {TimeString}, {"RecentFileAutomation".GetLocalized()}";
    }
}
