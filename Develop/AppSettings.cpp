#include "pch.h"
#include "AppSettings.h"
#if __has_include("AppSettings.g.cpp")
#include "AppSettings.g.cpp"
#endif
#include <winrt/DevelopManaged.h>

using namespace winrt;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Resources;
using namespace Windows::UI::StartScreen;
using namespace Windows::Storage;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
    bool dialogShown = false;

	DevelopManaged::SettingsViewModel preferences = nullptr;

    ResourceLoader stringLoader = nullptr;

    JumpList AppJumpList = nullptr;

	IVector<hstring> supportedFileTypes;

	IVectorView<hstring> AppSettings::SupportedFileTypes() { return supportedFileTypes.GetView(); }

    DevelopManaged::SettingsViewModel AppSettings::Preferences()
    {
        if (preferences == nullptr) preferences = DevelopManaged::SettingsViewModel();
        return preferences; 
    }

	IAsyncAction AppSettings::InitializeAsync()
	{
		auto assetsFolder = co_await Package::Current().InstalledLocation().GetFolderAsync(L"Assets");
		auto fileTypesFile = co_await assetsFolder.GetFileAsync(L"FileTypes");
		supportedFileTypes = co_await FileIO::ReadLinesAsync(fileTypesFile);
        if (JumpList::IsSupported())
        {
            AppJumpList = co_await JumpList::LoadCurrentAsync();
            AppJumpList.SystemGroupKind(JumpListSystemGroupKind::None);
            AppJumpList.Items().Clear();
            try { co_await AppJumpList.SaveAsync(); }
            catch (winrt::hresult_error ex)
            {
                if (ex.code() != 0x80070497) throw ex;
            }
        }
	}

    hstring AppSettings::GetLocalized(hstring const& key)
    {
        if (stringLoader == nullptr) stringLoader = ResourceLoader::GetForViewIndependentUse();
        return stringLoader.GetString(key);
    }

    bool AppSettings::DialogShown() { return dialogShown; }

    void AppSettings::DialogShown(bool value) { dialogShown = value; }
}