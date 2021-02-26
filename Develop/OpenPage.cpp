#include "OpenPage.h"
#if __has_include("OpenPage.g.cpp")
#include "OpenPage.g.cpp"
#endif
#include "Stopwatch.hpp"

using namespace winrt;
using namespace JUVStudios;
using namespace std::chrono_literals;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Storage;
using namespace Windows::Storage::Pickers;
using namespace Windows::Storage::BulkAccess;
using namespace Windows::Storage::FileProperties;
using namespace Windows::Storage::Search;
using namespace Windows::UI::Xaml::Media::Imaging;

namespace winrt::Develop::implementation
{
	OpenPage::OpenPage()
	{
		InitializeComponent();
	}

	IAsyncAction OpenPage::LoadItemList()
	{
		stopwatch::Stopwatch watch;
		StopTrackingChanges();
		Loading(true);
		SelectFolderMessage().Visibility(Visibility::Collapsed);
		PathBox().Text(m_CurrentFolder.Path());

		QueryOptions options;
		options.FolderDepth(FolderDepth::Shallow);
		options.IndexerOption(IndexerOption::UseIndexerWhenAvailable);

		QueryOptions deepQueryOptions;
		deepQueryOptions.FolderDepth(FolderDepth::Deep);
		deepQueryOptions.IndexerOption(IndexerOption::OnlyUseIndexer);

		m_CurrentQuery = m_CurrentFolder.CreateItemQueryWithOptions(options);
		auto itemsList = co_await m_CurrentQuery.GetItemsAsync();
		std::vector<ListedStorageItem> storageItems;
		storageItems.reserve(itemsList.Size());
		BitmapImage thumbnail = nullptr;
		hstring itemSizeDesc;
		for (auto&& item : itemsList)
		{
			auto file = item.try_as<StorageFile>();
			bool isFolder = item.IsOfType(StorageItemTypes::Folder);
			auto properties = item.as<IStorageItemProperties2>();
			if (isFolder && m_FallbackThumbnails.FolderThumbnail != nullptr) thumbnail = m_FallbackThumbnails.FolderThumbnail;
			else
			{
				bool supportedFileType = file != nullptr ? AppSettings::IsFileTypeSupported(file.FileType()) : false;
				if (!isFolder && !supportedFileType) continue;
				if (supportedFileType && m_FallbackThumbnails.SupportedFileThumbnail != nullptr) thumbnail = m_FallbackThumbnails.SupportedFileThumbnail;
				else
				{
					if (supportedFileType)
					{
						m_FallbackThumbnails.SupportedFileThumbnail = BitmapImage(Uri(L"ms-appx:///Assets/icons8-code-file-48.png"));
						thumbnail = m_FallbackThumbnails.SupportedFileThumbnail;
					}
					else
					{
						try
						{
							auto thumbnailStream = co_await properties.GetThumbnailAsync(ThumbnailMode::DocumentsView, 48, ThumbnailOptions::None);
							thumbnail = BitmapImage();
							co_await thumbnail.SetSourceAsync(thumbnailStream);
							if (isFolder) m_FallbackThumbnails.FolderThumbnail = thumbnail;
						}
						catch (hresult_invalid_argument const&)
						{
							if (m_FallbackThumbnails.FileThumbnail == nullptr) m_FallbackThumbnails.FileThumbnail = BitmapImage(Uri(L"ms-appx:///Assets/icons8-file-48.png"));
							thumbnail = m_FallbackThumbnails.FileThumbnail;
						}
					}
				}
			}

			if (!isFolder) itemSizeDesc = Helpers::HumanizeFileSize((co_await item.GetBasicPropertiesAsync()).Size(), false);
			else
			{
				auto folder = item.as<StorageFolder>();
				if (co_await folder.GetIndexedStateAsync() != IndexedState::FullyIndexed) itemSizeDesc = ResourceController::GetTranslation(L"NotAvailableMessage");
				else
				{
					auto folder = item.as<StorageFolder>();
					itemSizeDesc = Helpers::IndexStringFormat(ResourceController::GetTranslation(L"DirectoryFoldersFilesCountText"), 
						{ 
							box_value<uint64_t>(co_await folder.CreateFolderQueryWithOptions(deepQueryOptions).GetItemCountAsync()), 
							box_value<uint64_t>(co_await folder.CreateFileQueryWithOptions(deepQueryOptions).GetItemCountAsync())
						});
				}
			}

			storageItems.emplace_back(item.as<IStorageItem2>(), thumbnail, itemSizeDesc);
		}

		m_StorageItemsList.ReplaceAll(storageItems);
		Loading(false);
		m_QueryChangedToken = m_CurrentQuery.ContentsChanged({ this, &OpenPage::StorageItemsChanged });
		OutputDebugStringW((L"Folder enumeration in " + m_CurrentFolder.Path() + L" took " + std::to_wstring(watch.elapsed<stopwatch::milliseconds>()).c_str() + L"ms\r\n").c_str());
	}

	bool OpenPage::Loading() { return !LoadBar().ShowPaused(); }

	void OpenPage::Loading(bool value) 
	{
		if (Loading() != value)
		{
			LoadBar().ShowPaused(!value);
			LoadBar().Visibility(value ? Visibility::Visible : Visibility::Collapsed);
			auto contentVisibility = value ? Visibility::Collapsed : Visibility::Visible;
			if (m_StorageItemsList.Size() == 0) FolderEmptyMessage().Visibility(contentVisibility);
			else ItemsGrid().Visibility(contentVisibility);
			AppBar().Visibility(contentVisibility);
			PathBox().Visibility(contentVisibility);
			m_Bindable.RaisePropertyChanged(wnameof(CanInteract));
		}
	}

	fire_and_forget OpenPage::BrowseLocationTask()
	{
		auto folderPicker = FolderPicker();
		folderPicker.SuggestedStartLocation(PickerLocationId::DocumentsLibrary);
		folderPicker.ViewMode(PickerViewMode::List);
		folderPicker.FileTypeFilter().Append(L"*");
		auto folder = co_await folderPicker.PickSingleFolderAsync();
		AppSettings::DialogShown(false);
		if (folder != nullptr && folder.Path() != PathBox().Text())
		{
			m_CurrentFolder = folder;
			m_FutureList.AddOrReplace(L"OpenPagePreload", folder);
			LoadItemList();
		}
	}

	bool OpenPage::CanInteract() { return !Loading() && PathBox().Text() != L"\\"; }

	IObservableVector<ListedStorageItem> OpenPage::StorageItemsList() { return m_StorageItemsList; }

	void OpenPage::TemporaryClear()
	{
		StopTrackingChanges();
		m_StorageItemsList.Clear();
	}

	void OpenPage::StopTrackingChanges()
	{
		if (m_CurrentQuery != nullptr)
		{
			if (m_QueryChangedToken)
			{
				m_CurrentQuery.ContentsChanged(m_QueryChangedToken.value());
				m_QueryChangedToken.reset();
			}

			m_CurrentQuery = nullptr;
		}
	}

	void OpenPage::Page_Unloaded(IInspectable const&, RoutedEventArgs const&)
	{
		m_Unloaded = true;
		if (!Loading()) TemporaryClear();
	}

	IAsyncAction OpenPage::Page_Loaded(IInspectable const&, RoutedEventArgs const&)
	{
		if (!Loading())
		{
			if (PathBox().Text() != L"\\") LoadItemList();
			else
			{
				if (m_FutureList.ContainsItem(L"OpenPagePreload"))
				{
					try
					{
						m_CurrentFolder = co_await m_FutureList.GetFolderAsync(L"OpenPagePreload");
						LoadItemList();
					}
					catch (hresult_error const&) { ClearList_Click(nullptr, nullptr); }
				}
				else if (!Windows::Storage::ApplicationData::Current().LocalSettings().Values().HasKey(L"NoPreloadFallback"))
				{
					try
					{
						m_CurrentFolder = co_await StorageFolder::GetFolderFromPathAsync(UserDataPaths::GetDefault().Documents());
						LoadItemList();
					}
					catch (hresult_access_denied const&) { ClearList_Click(nullptr, nullptr); }
				}
				else ClearList_Click(nullptr, nullptr);
			}
		}
	}

	void OpenPage::ClearList_Click(IInspectable const&, RoutedEventArgs const&)
	{
		if (!Loading())
		{
			TemporaryClear();
			PathBox().Text(L"\\");
			m_Bindable.RaisePropertyChanged(wnameof(CanInteract));
			SelectFolderMessage().Visibility(Visibility::Visible);
			FolderEmptyMessage().Visibility(Visibility::Collapsed);
			PathBox().Visibility(Visibility::Collapsed);
			AppBar().Visibility(Visibility::Collapsed);
			if (m_FutureList.ContainsItem(L"OpenPagePreload")) m_FutureList.Remove(L"OpenPagePreload");
			ApplicationData::Current().LocalSettings().Values().Insert(L"NoPreloadFallback", box_value(1));
		}
	}

	IAsyncAction OpenPage::Open_Click(IInspectable const&, RoutedEventArgs const&)
	{
		if (!AppSettings::DialogShown())
		{
			AppSettings::DialogShown(true);
			auto filePicker = FileOpenPicker();
			filePicker.SuggestedStartLocation(PickerLocationId::DocumentsLibrary);
			filePicker.ViewMode(PickerViewMode::List);
			for (auto&& fileType : AppSettings::SupportedFileTypes()) filePicker.FileTypeFilter().Append(fileType);
			auto file = co_await filePicker.PickSingleFileAsync();
			AppSettings::DialogShown(false);
			if (file != nullptr)
			{
				auto mainPage = Window::Current().Content().as<Develop::MainPage>();
				mainPage.ViewModel().AddStorageItems({ file });
			}
		}
	}

	void OpenPage::ToggleSelection_Click(IInspectable const& sender, RoutedEventArgs const&)
	{
		auto target = sender.as<AppBarToggleButton>();
		auto currentSelection = ItemsGrid().SelectedItems();
		com_array<IInspectable> selectionList { currentSelection.Size() };
		for (uint32_t i = 0; i < selectionList.size(); i++) selectionList[i] = currentSelection.GetAt(0);
		ItemsGrid().SelectionMode(ItemsGrid().SelectionMode() == ListViewSelectionMode::Multiple ? ListViewSelectionMode::Extended : ListViewSelectionMode::Multiple);
		ItemsGrid().SelectedItems().ReplaceAll(selectionList);
		target.IsChecked(ItemsGrid().SelectionMode() == ListViewSelectionMode::Multiple);
	}

	void OpenPage::PickFolder_Click(IInspectable const&, RoutedEventArgs const&)
	{
		if (!AppSettings::DialogShown() && !Loading())
		{
			AppSettings::DialogShown(true);
			BrowseLocationTask();
		}
	}

	IAsyncAction OpenPage::StorageItemsChanged(IStorageQueryResultBase const& sender, IInspectable const&)
	{
		co_await resume_foreground(Dispatcher());
		if (sender.Folder().IsEqual(m_CurrentFolder) && !Loading() && !AppSettings::DialogShown()) LoadItemList();
	}

	void OpenPage::ItemsGrid_DoubleTapped(IInspectable const&, Input::DoubleTappedRoutedEventArgs const&)
	{
		auto selection = ItemsGrid().SelectedItems();
		if (selection.Size() > 0)
		{
			auto itemsToLaunch = CollectionAs<IStorageItem2, IInspectable>(selection, [](IInspectable unknown)
				{
					return unknown.as<ListedStorageItem>().ItemRef(); 
				});

			if (itemsToLaunch.Size() == 1 && itemsToLaunch.GetAt(0).IsOfType(StorageItemTypes::Folder))
			{
				m_CurrentFolder = itemsToLaunch.GetAt(0).as<StorageFolder>();
				m_FutureList.AddOrReplace(L"OpenPagePreload", m_CurrentFolder);
				LoadItemList();
			}
			else Window::Current().Content().as<MainPage>().ViewModel().AddStorageItems(itemsToLaunch.GetView());
		}
	}
}