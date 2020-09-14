using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectCodeEditor.Services
{
    internal static class LogService
    {
        public static async Task LogException(Exception e)
        {
            var folder = await ApplicationData.Current.TemporaryFolder.GetItemAsync("crashes");
            if (folder != null)
            {
                var dateTime = DateTime.Now;
                var time = dateTime.TimeOfDay;
                var logFile = await (folder as StorageFolder).CreateFileAsync(dateTime.Date.ToLongDateString() + time.ToString());
                var infoToWrite = new string[]
                {
                    e.Source,
                    e.StackTrace,
                    e.InnerException.Message,
                    e.Data.ToString(),
                    e.HResult.ToString(),
                    e.HelpLink,
                    e.HResult.ToString(),
                    e.Message
                };

                await FileIO.WriteLinesAsync(logFile, infoToWrite);
            }
            else
            {
                await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("crashes");
                LogException(e).GetAwaiter().GetResult();
            }
        }
    }
}
