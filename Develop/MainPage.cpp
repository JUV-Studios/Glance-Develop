#include "pch.h"
#include "MainPage.h"
#include "MainPage.g.cpp"
#include "AppSettings.h"
#include "ShellViewModel.h"
#include "App.h"

using namespace winrt;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::Storage;
using namespace Windows::Storage::Pickers;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::ApplicationModel::Core;

namespace winrt::Develop::implementation
{
	MainPage::MainPage()
	{
		InitializeComponent();
		m_AppView = CoreApplication::GetCurrentView();
		m_AppView.TitleBar().ExtendViewIntoTitleBar(true);
	}

	void MainPage::UserControl_Loaded(IInspectable const&, RoutedEventArgs const&)
	{
		Window::Current().SetTitleBar(DragRegion());
	}

	Develop::ShellViewModel MainPage::ViewModel() { return m_ViewModel; }

	void MainPage::AboutItem_Loaded(IInspectable const& sender, RoutedEventArgs const&)
	{
		auto target = sender.as<Controls::MenuFlyoutItem>();
		if (target.Text().empty()) target.Text(AppSettings::GetLocalized(L"AboutDialog/Title"));
	}

	void MainPage::AboutItem_Click(IInspectable const&, RoutedEventArgs const&) { AboutDialog::ShowDialogAsync(); }

	void MainPage::AppMenu_Click(IInspectable const&, RoutedEventArgs const&)
	{
		Controls::Primitives::FlyoutShowOptions options;
		options.Position(Point(0, 0));
		Resources().Lookup(box_value(L"AppFlyout")).as<Controls::MenuFlyout>().ShowAt(nullptr, options);
	}

	fire_and_forget MainPage::OpenFile()
	{
		if (m_OpenPicker == nullptr)
		{
			m_OpenPicker = FileOpenPicker();
			m_OpenPicker.SuggestedStartLocation(PickerLocationId::DocumentsLibrary);
			m_OpenPicker.ViewMode(PickerViewMode::List);
			for (auto&& fileType : AppSettings::SupportedFileTypes()) m_OpenPicker.FileTypeFilter().Append(fileType);
		}

		auto files = co_await m_OpenPicker.PickMultipleFilesAsync();
		if (files != nullptr && files.Size() > 0)
		{
			m_ViewModel.AddStorageItems(collection_view_as<IStorageItem2>(files));
		}
	}

	void MainPage::OpenFile_Click(IInspectable const&, RoutedEventArgs const&) { OpenFile(); }

	void MainPage::OpenProject_Click(IInspectable const&, RoutedEventArgs const&)
	{

	}

	void MainPage::AppFlyout_Opening(IInspectable const& sender, IInspectable const& e)
	{

		if (m_ViewModel.SelectedInstance().Content())
		{

		}
	}
}