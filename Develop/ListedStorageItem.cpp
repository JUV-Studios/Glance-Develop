#include "ListedStorageItem.h"
#if __has_include("ListedStorageItem.g.cpp")
#include "ListedStorageItem.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media::Imaging;

namespace winrt::Develop::implementation
{
	ListedStorageItem::ListedStorageItem(IStorageItem2 const& item, Windows::UI::Xaml::Media::Imaging::BitmapImage const& thumbnail, hstring const& sizeDescription)
		: m_Item(item), m_Thumbnail(thumbnail), m_SizeDescription(sizeDescription) {}

	IStorageItem2 ListedStorageItem::ItemRef() { return m_Item; }

	BitmapImage ListedStorageItem::Thumbnail() { return m_Thumbnail; }

	hstring ListedStorageItem::SizeDescription() { return m_SizeDescription; }

	hstring ListedStorageItem::ItemType()
	{
		if (m_Properties == nullptr) m_Properties = ItemRef().as<IStorageItemProperties>();
		return m_Properties.DisplayType();
	}

	hstring ListedStorageItem::ToString() { return m_Item.Name() + L", " + SizeDescription() + L", " + ItemType(); }
}
