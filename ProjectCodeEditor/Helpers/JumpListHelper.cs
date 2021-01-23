using System;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace ProjectCodeEditor.Helpers
{
    public static class JumpListHelper
    {
        public static JumpList AppJumpList;

        public static async Task InitializeAsync()
        {
            if (JumpList.IsSupported())
            {
                AppJumpList = await JumpList.LoadCurrentAsync();
                AppJumpList.SystemGroupKind = JumpListSystemGroupKind.None;
                AppJumpList.Items.Clear();
                var openItem = JumpListItem.CreateWithArguments("OpenFiles", "ms-resource:///Resources/OpenFilesOption/Title");
                openItem.Description = "ms-resource:///Resources/OpenFilesOption/Description";
                var newItem = JumpListItem.CreateWithArguments("NewFiles", "ms-resource:///Resources/NewFileOption/Title");
                newItem.Description = "ms-resource:///Resources/NewFileOption/Description";
                AppJumpList.Items.Add(openItem);
                AppJumpList.Items.Add(newItem);
                try
                {
                    await AppJumpList.SaveAsync();
                }
                catch (Exception ex)
                {
                    if (ex.HResult != 80070497) throw ex;
                }
            }
        }
    }
}
