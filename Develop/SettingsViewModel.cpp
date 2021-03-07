#include "SettingsViewModel.h"
#if __has_include("SettingsViewModel.g.cpp")
#include "SettingsViewModel.g.cpp"
#endif

using namespace winrt;
using namespace JUVStudios;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::Foundation;
using namespace Windows::ApplicationModel;

namespace winrt::Develop::implementation
{
	hstring m_AboutText;

	Develop::SettingsViewModel instanceRef = nullptr;

	Develop::SettingsViewModel SettingsViewModel::Instance()
	{
		if (instanceRef == nullptr) instanceRef = make<SettingsViewModel>();
		return instanceRef;
	}

	hstring SettingsViewModel::AboutText()
	{
		if (m_AboutText.empty())
		{
			auto packageVersion = Package::Current().Id().Version();
			auto versionString = Helpers::IndexStringFormat(L"{0}.{1}.{2}.{3}",
				{
					box_value(packageVersion.Major),
					box_value(packageVersion.Minor),
					box_value(packageVersion.Build),
					box_value(packageVersion.Revision)
				});

			m_AboutText = Helpers::IndexStringFormat(L"Develop\r{0}\r{1}\r{2}",
				{
					box_value(versionString),
					box_value(Helpers::GetResourceTranslation(L"DevelopedBlock/Text")),
					box_value(Helpers::GetResourceTranslation(L"CopyrightBlock/Text"))
				});
		}

		return m_AboutText;
	}

	hstring SettingsViewModel::FontFamily()
	{
		return unbox_value<hstring>(Helpers::GetAppSetting(L"FontFamily", box_value(L"Consolas")));
	}

	void SettingsViewModel::FontFamily(hstring const& value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(L"FontFamily", box_value(value));
	}

	uint32_t SettingsViewModel::FontSize()
	{
		return unbox_value<uint32_t>(Helpers::GetAppSetting(L"FontSize", box_value(18u)));
	}

	void SettingsViewModel::FontSize(uint32_t value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(L"FontSize", box_value(value));
	}

	bool SettingsViewModel::AutoSave()
	{
		return unbox_value<bool>(Helpers::GetAppSetting(L"AutoSave", box_value(false)));
	}

	void SettingsViewModel::AutoSave(bool value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(L"AutoSave", box_value(value));
	}

	bool SettingsViewModel::DisableSound()
	{
		return unbox_value<bool>(Helpers::GetAppSetting(L"DisableSound", box_value(false)));
	}

	void SettingsViewModel::DisableSound(bool value)
	{
		ApplicationData::Current().LocalSettings().Values().Insert(L"DisableSound", box_value(value));
		ElementSoundPlayer::State(value ? ElementSoundPlayerState::Off : ElementSoundPlayerState::On);
	}

	IInspectable SettingsViewModel::GetHolder() const noexcept
	{
		return *this;
	}
}