#include "pch.h"
#include "Preferences.h"

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::ApplicationModel;
using namespace Windows::Data::Json;
using namespace Windows::Storage;
using namespace Windows::Foundation::Collections;

IVector<hstring> supportedFileTypes;

IVectorView<hstring> Preferences::SupportedFileTypes() { return supportedFileTypes.GetView(); }

IAsyncAction Preferences::Initialize()
{
	auto assetsFolder = co_await Package::Current().InstalledLocation().GetFolderAsync(L"Assets");
	auto fileTypesFile = co_await assetsFolder.GetFileAsync(L"FileTypes");
	supportedFileTypes = co_await FileIO::ReadLinesAsync(fileTypesFile);
}