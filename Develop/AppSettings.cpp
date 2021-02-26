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

	std::deque<Develop::IAsyncClosable> closeList;

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

	void AppSettings::AddToCloseList(Develop::IAsyncClosable const& view)
	{
		closeList.push_back(view);
	}

	void AppSettings::RemoveFromCloseList(Develop::IAsyncClosable const& view)
	{
		for (auto iter = closeList.begin(); iter != closeList.end();)
		{
			iter.operator*() == view ? iter = closeList.erase(iter) : iter++;
		}
	}

	bool AppSettings::IsFileTypeSupported(hstring const& fileType)
	{
		if (fileType == L"." || fileType.empty()) return true;
		std::wstring fileTypeLower = fileType.c_str();
		std::transform(fileTypeLower.begin(), fileTypeLower.end(), fileTypeLower.begin(), [](auto c) { return std::towlower(c); });
		for (auto const& type : supportedFileTypes)
		{
			if (type == fileTypeLower) return true;
			else continue;
		}

		return false;
	}
}