#include "pch.h"
#include "AppSettings.h"
#if __has_include("AppSettings.g.cpp")
#include "AppSettings.g.cpp"
#endif

using namespace winrt;
using namespace Windows::ApplicationModel;
using namespace Windows::Storage;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
	DevelopManaged::SettingsViewModel preferences;
	IVector<hstring> supportedFileTypes;

	IVectorView<hstring> AppSettings::SupportedFileTypes() { return supportedFileTypes.GetView(); }

	DevelopManaged::SettingsViewModel AppSettings::Preferences() { return preferences; }

	IAsyncAction AppSettings::Initialize()
	{
		auto assetsFolder = co_await Package::Current().InstalledLocation().GetFolderAsync(L"Assets");
		auto fileTypesFile = co_await assetsFolder.GetFileAsync(L"FileTypes");
		supportedFileTypes = co_await FileIO::ReadLinesAsync(fileTypesFile);
	}
}
