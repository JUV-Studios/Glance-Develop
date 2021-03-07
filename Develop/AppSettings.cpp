#include "AppSettings.h"
#if __has_include("AppSettings.g.cpp")
#include "AppSettings.g.cpp"
#endif

using namespace winrt;
using namespace JUVStudios;
using namespace Windows::ApplicationModel;
using namespace Windows::Data::Xml::Dom;
using namespace Windows::Storage;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{	
	bool dialogShown = false;

	IVector<hstring> supportedFileTypes;

	IVectorView<hstring> AppSettings::SupportedFileTypes() 
	{
		return supportedFileTypes.GetView(); 
	}

	IAsyncAction AppSettings::InitializeAsync()
	{
		std::vector<hstring> fileTypesList;
		XmlDocument packageManifest;
		packageManifest.LoadXml(co_await PathIO::ReadTextAsync(L"ms-appx:///AppxManifest.xml"));
		auto extensionsList = packageManifest.ChildNodes().GetAt(1).ChildNodes().GetAt(13).ChildNodes().GetAt(1).ChildNodes().GetAt(3);
		auto associations = extensionsList.ChildNodes().GetAt(1).ChildNodes().GetAt(1).ChildNodes().GetAt(1);
		for (auto&& plainType : associations.ChildNodes())
		{
			auto text = plainType.InnerText();
			if (!text.empty()) fileTypesList.push_back(text);
		}

		fileTypesList.push_back(L".rtf");
		supportedFileTypes = single_threaded_vector(std::move(fileTypesList));
	}

	bool AppSettings::DialogShown() { return dialogShown; }

	void AppSettings::DialogShown(bool value) { dialogShown = value; }
}