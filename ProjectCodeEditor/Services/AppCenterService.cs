using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace ProjectCodeEditor.Services
{
    public sealed class AppCenterService
    {
        public static void Initialize()
        {
            AppCenter.Start("dd9a81de-fe79-4ab8-be96-8f96c346c88e", typeof(Analytics), typeof(Crashes));
        }
    }
}
