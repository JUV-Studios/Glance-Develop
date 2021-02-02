using System;
using Windows.UI.StartScreen;

namespace DevelopManaged
{
    public static class JumpListHelper
    {
        private static JumpList AppJumpList = null;

        public static async void InitializeAsync()
        {
            if (JumpList.IsSupported())
            {
                AppJumpList = await JumpList.LoadCurrentAsync();
                AppJumpList.SystemGroupKind = JumpListSystemGroupKind.None;
                AppJumpList.Items.Clear();
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
