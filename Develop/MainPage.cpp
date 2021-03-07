#include "MainPage.h"
#include "MainPage.g.cpp"
#include "App.h"

using namespace winrt;
using namespace JUVStudios;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::System;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
	MainPage::MainPage()
	{
		InitializeComponent();
		CoreApplication::GetCurrentView().CoreWindow().Dispatcher().AcceleratorKeyActivated({ this, &MainPage::KeyPressHandler });
		ElementSoundPlayer::State(SettingsViewModel::Instance().DisableSound() ? ElementSoundPlayerState::Off : ElementSoundPlayerState::On);
	}

	void MainPage::GoStart()
	{
		auto homePageInstance = ShellViewModel::Instance().Instances().GetAt(FindSingle<ShellView>(ShellViewModel::Instance().Instances().GetView(), 0, [](auto instance)
			{
				return get_class_name(instance.Content()) == L"Develop.HomePage";
			}));

		ShellViewModel::Instance().SelectedInstance(homePageInstance);
		auto homePageContent = homePageInstance.Content().as<HomePage>();
		homePageContent.SetPageIndex(0);
	}

	void MainPage::GoOpen()
	{
		auto homePageInstance = ShellViewModel::Instance().Instances().GetAt(FindSingle<ShellView>(ShellViewModel::Instance().Instances().GetView(), 0, [](auto instance)
			{
				return get_class_name(instance.Content()) == L"Develop.HomePage";
			}));

		ShellViewModel::Instance().SelectedInstance(homePageInstance);
		auto homePageContent = homePageInstance.Content().as<HomePage>();
		homePageContent.SetPageIndex(1);
	}

	void MainPage::KeyPressHandler(Windows::UI::Core::CoreDispatcher const&, Windows::UI::Core::AcceleratorKeyEventArgs const& e)
	{
		if (!AppSettings::DialogShown() && e.EventType() == CoreAcceleratorKeyEventType::KeyDown && IsInKeyDown(VirtualKey::Control))
		{
			switch (e.VirtualKey())
			{
			case VirtualKey::Tab:
				// Handle Control + Tab
				e.Handled(true);
				ShellViewModel::Instance().FastSwitch(IsInKeyDown(VirtualKey::Shift));
				break;

			case VirtualKey::W:
				// Handle Ctrl + W
				ShellViewModel::Instance().TryCloseInstance(ShellViewModel::Instance().SelectedInstance());
				break;

			case VirtualKey::F4:
				// Handle Ctrl + F4
				ShellViewModel::Instance().TryCloseInstance(ShellViewModel::Instance().SelectedInstance());
				break;

			case VirtualKey::O:
				// Handle Ctrl + O
				e.Handled(true);
				GoOpen();
				break;

			default:
				// Don't handle the event
				e.Handled(false);
				break;
			}
		}
	}

	void MainPage::CloseCurrentTab_Click(IInspectable const&, RoutedEventArgs const&) 
	{
		auto selected = ShellViewModel::Instance().SelectedInstance();
		JUVStudios::IAsyncClosable closable;
		if (selected.Content().try_as(closable))
		{
			if (closable.StartClosing()) ShellViewModel::Instance().RemoveInstance(selected);
		}
	}
}