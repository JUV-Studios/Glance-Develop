#include "pch.h"
#include "MainPage.h"
#include "MainPage.g.cpp"
#include "App.h"

using namespace winrt;
using namespace Windows::UI::Core;
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
		m_AppView.TitleBar().ExtendViewIntoTitleBar(false);
		m_AppView.CoreWindow().Dispatcher().AcceleratorKeyActivated({ this, &MainPage::KeyPressHandler });
		ElementSoundPlayer::State(SettingsViewModel::Instance().DisableSound() ? ElementSoundPlayerState::Off : ElementSoundPlayerState::On);
	}

	ShellViewModel MainPage::ViewModel() { return m_ViewModel; }

	void MainPage::KeyPressHandler(CoreDispatcher const&, AcceleratorKeyEventArgs const& e)
	{
		if (e.EventType() == CoreAcceleratorKeyEventType::KeyDown 
			&& (m_AppView.CoreWindow().GetKeyState(Windows::System::VirtualKey::Control) & CoreVirtualKeyStates::Down) == CoreVirtualKeyStates::Down 
			&& e.VirtualKey() == Windows::System::VirtualKey::Tab)
		{
			// Handle Control + Tab
			e.Handled(true);
			uint32_t index = 0;
			m_ViewModel.Instances().IndexOf(m_ViewModel.SelectedInstance(), index);
			if (m_ViewModel.Instances().Size() != 1)
			{
				if (index == m_ViewModel.Instances().Size() - 1) m_ViewModel.SelectedInstance(m_ViewModel.Instances().GetAt(0));
				else m_ViewModel.SelectedInstance(m_ViewModel.Instances().GetAt(index + 1));
			}
		}
	}

	void MainPage::AboutItem_Loaded(IInspectable const& sender, RoutedEventArgs const&)
	{
		auto target = sender.as<Controls::MenuFlyoutItem>();
		if (target.Text().empty()) target.Text(JUVStudios::ResourceController::GetTranslation(L"AboutDialog/Title"));
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
		if (!AppSettings::DialogShown())
		{
			AppSettings::DialogShown(true);
			if (m_OpenPicker == nullptr)
			{
				m_OpenPicker = FileOpenPicker();
				m_OpenPicker.SuggestedStartLocation(PickerLocationId::DocumentsLibrary);
				m_OpenPicker.ViewMode(PickerViewMode::List);
				m_OpenPicker.FileTypeFilter().Append(L"*");
				for (auto&& fileType : AppSettings::SupportedFileTypes()) m_OpenPicker.FileTypeFilter().Append(fileType);
			}

			auto files = co_await m_OpenPicker.PickMultipleFilesAsync();
			AppSettings::DialogShown(false);
			if (files != nullptr && files.Size() > 0)
			{
				m_ViewModel.AddStorageItems(::JUVStudios::CollectionAs<IStorageItem2>(files).GetView());
			}
		}
	}

	void MainPage::OpenFile_Click(IInspectable const&, RoutedEventArgs const&) { OpenFile(); }

	void MainPage::OpenProject_Click(IInspectable const&, RoutedEventArgs const&)
	{

	}

	void MainPage::CloseCurrentTab_Click(IInspectable const&, RoutedEventArgs const&) 
	{
		auto selected = ViewModel().SelectedInstance();
		auto closable = selected.Content().as<IAsyncClosable>();
		if (closable.PrepareClose())
		{
			uint32_t index = 0;
			if (ViewModel().Instances().IndexOf(selected, index))
			{
				ViewModel().SelectedInstance(ViewModel().Instances().GetAt(0));
				ViewModel().Instances().RemoveAt(index);
				selected.Close();
				closable.CloseAsync();
			}
		}
	}

	void MainPage::JoinDiscordServer_Loaded(IInspectable const& sender, RoutedEventArgs const&)
	{
		auto target = sender.as<Controls::MenuFlyoutItem>();
		if (target.Text().empty()) target.Text(JUVStudios::ResourceController::GetTranslation(L"JoinDiscordServerLink/Content"));
	}

	void MainPage::JoinDiscordServer_Click(IInspectable const&, RoutedEventArgs const&)
	{
		Windows::System::Launcher::LaunchUriAsync(Uri(L"https://discord.com/invite/xKVWBWu"));
	}
}