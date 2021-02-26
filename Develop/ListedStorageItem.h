#pragma once
#include "App.h"
#include "ListedStorageItem.g.h"

namespace winrt::Develop::implementation
{
    struct ListedStorageItem : ListedStorageItemT<ListedStorageItem>
    {
        ListedStorageItem(Windows::Storage::IStorageItem2 const&, Windows::UI::Xaml::Media::Imaging::BitmapImage const&, hstring const&);
        Windows::Storage::IStorageItem2 ItemRef();
        Windows::UI::Xaml::Media::Imaging::BitmapImage Thumbnail();
        hstring SizeDescription();
        hstring ItemType();
        hstring ToString();
    private:
        const Windows::Storage::IStorageItem2 m_Item;
        const Windows::UI::Xaml::Media::Imaging::BitmapImage m_Thumbnail;
        const hstring m_SizeDescription;
        Windows::Storage::IStorageItemProperties m_Properties;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ListedStorageItem : ListedStorageItemT<ListedStorageItem, implementation::ListedStorageItem>
    {
    };
}
