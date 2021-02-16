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
    bool dialogShown = false;

	IVector<hstring> supportedFileTypes;

	IVectorView<hstring> AppSettings::SupportedFileTypes() { return supportedFileTypes.GetView(); }

	IAsyncAction AppSettings::InitializeAsync()
	{
		auto assetsFolder = co_await Package::Current().InstalledLocation().GetFolderAsync(L"Assets");
		auto fileTypesFile = co_await assetsFolder.GetFileAsync(L"FileTypes");
		supportedFileTypes = co_await FileIO::ReadLinesAsync(fileTypesFile);
	}

    bool AppSettings::DialogShown() { return dialogShown; }

    void AppSettings::DialogShown(bool value) { dialogShown = value; }
}