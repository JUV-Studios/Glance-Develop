#include "MainPage.h"
#include "MainPage.g.cpp"
#include "App.h"

using namespace winrt;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::System;
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

	hstring MainPage::JoinDiscordServerId() { return L"JoinDiscordServerLink/Content"; }

	CoreApplicationView MainPage::AppView() { return m_AppView; }

	void MainPage::KeyPressHandler(CoreDispatcher const&, AcceleratorKeyEventArgs const& e)
	{
		if (AppSettings::DialogShown()) return;
		if (e.EventType() == CoreAcceleratorKeyEventType::KeyDown && (m_AppView.CoreWindow().GetKeyState(VirtualKey::Control) & CoreVirtualKeyStates::Down) == CoreVirtualKeyStates::Down)
		{
			if (e.VirtualKey() == VirtualKey::Tab)
			{
				// Handle Control + Tab
				e.Handled(true);
				uint32_t index = m_ViewModel.SelectedIndex();
				if (m_ViewModel.Instances().Size() != 1)
				{
					if (index == m_ViewModel.Instances().Size() - 1) m_ViewModel.SelectedIndex(0);
					else m_ViewModel.SelectedIndex(index + 1);
				}
			}
			else if (e.VirtualKey() == VirtualKey::O)
			{
				// Handle Ctrl + O
				e.Handled(true);
				auto homePageInstance = m_ViewModel.Instances().GetAt(0);
				m_ViewModel.SelectedInstance(homePageInstance);
				auto homePageContent = homePageInstance.Content().as<HomePage>();
				homePageContent.SetPageIndex(0);
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

	void MainPage::OpenProject_Click(IInspectable const&, RoutedEventArgs const&)
	{

	}

	void MainPage::CloseCurrentTab_Click(IInspectable const&, RoutedEventArgs const&) 
	{
		auto selected = ViewModel().SelectedInstance();
		IAsyncClosable closable;
		if (selected.Content().try_as<IAsyncClosable>(closable))
		{
			if (closable.PrepareClose()) ViewModel().RemoveInstance(selected);
		}
	}

	void MainPage::JoinDiscordServer_Click(IInspectable const&, RoutedEventArgs const&)
	{
		Windows::System::Launcher::LaunchUriAsync(Uri(L"https://discord.com/invite/xKVWBWu"));
	}
}