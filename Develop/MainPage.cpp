#include "MainPage.h"
#include "MainPage.g.cpp"
#include <winrt/Shared.h>

using namespace winrt;
using namespace Shared;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::ViewManagement;
using namespace Windows::System;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::Foundation;

namespace winrt::Develop::implementation
{
	MainPage::MainPage()
	{
		InitializeComponent();
		CoreApplication::GetCurrentView().CoreWindow().Dispatcher().AcceleratorKeyActivated({ this, &MainPage::KeyPressHandler });
		ElementSoundPlayer::State(AppSettings::Preferences().DisableSound() ? ElementSoundPlayerState::Off : ElementSoundPlayerState::On);
	}

	void MainPage::KeyPressHandler(Windows::UI::Core::CoreDispatcher const&, Windows::UI::Core::AcceleratorKeyEventArgs const& e)
	{
		if (!AppSettings::DialogShown() && e.EventType() == CoreAcceleratorKeyEventType::KeyDown && 
			(CoreApplication::GetCurrentView().CoreWindow().GetKeyState(VirtualKey::Control) & CoreVirtualKeyStates::Down) == CoreVirtualKeyStates::Down)
		{
			switch (e.VirtualKey())
			{
			case VirtualKey::Tab:
				// Handle Control + Tab
				e.Handled(true);
				ShellViewModel::Instance().FastSwitch(CoreApplication::GetCurrentView().CoreWindow().GetKeyState(VirtualKey::Shift) == CoreVirtualKeyStates::Down);
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
				//GoOpen();
				break;

			default:
				// Don't handle the event
				e.Handled(false);
				break;
			}
		}
	}
}