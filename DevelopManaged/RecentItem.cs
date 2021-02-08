using Humanizer;
using System;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace DevelopManaged
{
    public sealed class RecentItem
    {
        public RecentItem(IStorageItem2 item, DateTimeOffset time, AccessListEntry entry)
        {
            Item = item;
            Time = time;
            Entry = entry;
        }

        public IStorageItem2 Item { get; private set; }

        public DateTimeOffset Time { get; private set; }

        public AccessListEntry Entry { get; private set; }

        public string FileLocation => General.FolderPath(Item.Path);

        public string TimeString => Time.DateTime.Humanize(false);

        public override string ToString() => $"{Item.Name}, {FileLocation}";
    }
}
