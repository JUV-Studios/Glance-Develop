using Microsoft.Toolkit.Uwp.Extensions;
using System;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace ProjectCodeEditor.Models
{
    public sealed record RecentFile(StorageFile File, DateTime Time, AccessListEntry Entry)
    {
        public string TimeString => $"{Time.ToShortTimeString()}";
        public override string ToString() => $"{File.Name}, {File.Path}, {TimeString}, {"RecentFileAutomation".GetLocalized()}";
    }
}
