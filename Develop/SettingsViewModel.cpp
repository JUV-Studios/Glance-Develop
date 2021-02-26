#include "SettingsViewModel.h"
#if __has_include("SettingsViewModel.g.cpp")
#include "SettingsViewModel.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::ApplicationModel;

namespace winrt::Develop::implementation
{
	Develop::SettingsViewModel instanceRef = nullptr;

	Develop::SettingsViewModel SettingsViewModel::Instance()
	{
		if (instanceRef == nullptr) instanceRef = make<SettingsViewModel>();
		return instanceRef;
	}

	event_token SettingsViewModel::PropertyChanged(PropertyChangedEventHandler const& handler) { return m_PropertyChanged.add(handler); }

	void SettingsViewModel::PropertyChanged(event_token const& token) noexcept { m_PropertyChanged.remove(token); }

	hstring SettingsViewModel::AboutText()
	{
		auto packageVersion = Package::Current().Id().Version();
		std::wstringstream versionString;
		versionString << JUVStudios::ResourceController::GetTranslation(L"VersionText").data() << L" ";
		versionString << packageVersion.Major << L".";
		versionString << packageVersion.Minor << L".";
		versionString << packageVersion.Build << L".";
		versionString << packageVersion.Revision << "\r";
		return (L"Develop\r" + versionString.str() + JUVStudios::ResourceController::GetTranslation(L"DevelopedBlock/Text").data() + L"\r" + JUVStudios::ResourceController::GetTranslation(L"CopyrightBlock/Text").data()).c_str();
	}

	hstring SettingsViewModel::FontFamily()
	{
		return unbox_value<hstring>(JUVStudios::Helpers::GetAppSetting(wnameof(FontFamily), box_value(L"Consolas")));
	}

	void SettingsViewModel::FontFamily(hstring const& value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(wnameof(FontFamily), box_value(value));
	}

	uint32_t SettingsViewModel::FontSize()
	{
		return unbox_value<uint32_t>(JUVStudios::Helpers::GetAppSetting(wnameof(FontSize), box_value(18u)));
	}

	void SettingsViewModel::FontSize(uint32_t value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(wnameof(FontSize), box_value(value));
	}

	bool SettingsViewModel::AutoSave()
	{
		return unbox_value<bool>(JUVStudios::Helpers::GetAppSetting(wnameof(AutoSave), box_value(false)));
	}

	void SettingsViewModel::AutoSave(bool value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(wnameof(AutoSave), box_value(value));
	}

	bool SettingsViewModel::DisableSound()
	{
		return unbox_value<bool>(JUVStudios::Helpers::GetAppSetting(wnameof(DisableSound), box_value(false)));
	}

	void SettingsViewModel::DisableSound(bool value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(wnameof(DisableSound), box_value(value));
		ElementSoundPlayer::State(value ? ElementSoundPlayerState::Off : ElementSoundPlayerState::On);
	}
}