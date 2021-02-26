#pragma once
#include "App.h"
#include "OpenPage.g.h"

namespace winrt::Develop::implementation
{
    struct ItemFallbackThumbnails
    {
        Windows::UI::Xaml::Media::Imaging::BitmapImage FolderThumbnail { nullptr };
        Windows::UI::Xaml::Media::Imaging::BitmapImage FileThumbnail { nullptr };
        Windows::UI::Xaml::Media::Imaging::BitmapImage SupportedFileThumbnail { nullptr };
    };

    struct OpenPage : OpenPageT<OpenPage>
    {
    private:
        Windows::Foundation::IAsyncAction LoadItemList();
        fire_and_forget BrowseLocationTask();
        bool Loading();
        void Loading(bool value);
        bool m_Unloaded = true;
        Windows::Storage::StorageFolder m_CurrentFolder = nullptr;
        Windows::Storage::Search::StorageItemQueryResult m_CurrentQuery = nullptr;
        std::optional<event_token> m_QueryChangedToken;
        ItemFallbackThumbnails m_FallbackThumbnails;
        const Windows::Storage::AccessCache::StorageItemAccessList m_FutureList = Windows::Storage::AccessCache::StorageApplicationPermissions::FutureAccessList();
        const Windows::Foundation::Collections::IObservableVector<Develop::ListedStorageItem> m_StorageItemsList = single_threaded_observable_vector<Develop::ListedStorageItem>();
        const JUVStudios::BindableObject m_Bindable{ *this };
    public:
        OpenPage();
        PropertyChangedHandler(m_Bindable);
        bool CanInteract();
        Windows::Foundation::Collections::IObservableVector<Develop::ListedStorageItem> StorageItemsList();
        void TemporaryClear();
        void StopTrackingChanges();
        void ClearList_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void ToggleSelection_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void PickFolder_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void Page_Unloaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        Windows::Foundation::IAsyncAction Page_Loaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        Windows::Foundation::IAsyncAction Open_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        Windows::Foundation::IAsyncAction StorageItemsChanged(Windows::Storage::Search::IStorageQueryResultBase const& sender, Windows::Foundation::IInspectable const& e);
        void ItemsGrid_DoubleTapped(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::Input::DoubleTappedRoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct OpenPage : OpenPageT<OpenPage, implementation::OpenPage>
    {
    };
}
