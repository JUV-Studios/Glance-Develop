using Humanizer;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using ProjectCodeEditor.Services;
using System;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace ProjectCodeEditor.Models
{
    public sealed class RecentItem : ObservableObject
    {
        public IStorageItem2 Item { get; init; }

        public DateTime Time { get; init; }

        public AccessListEntry Entry { get; init; }

        public string FileLocation => FileService.GetFolderPath(Item);

        public string TimeString => Time.Humanize(false);

        public override string ToString() => $"{Item.Name}, {FileLocation}, {TimeString}";
    }
}
