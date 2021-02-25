#pragma once
#include "App.h"
#include "SettingsViewModel.g.h"

namespace winrt::Develop::implementation
{
    struct SettingsViewModel : SettingsViewModelT<SettingsViewModel>
    {
        SettingsViewModel() = default;
        static Develop::SettingsViewModel Instance();
        static hstring AboutText();
        hstring FontFamily();
        void FontFamily(hstring const& value);
        uint32_t FontSize();
        void FontSize(uint32_t value);
        bool AutoSave();
        void AutoSave(bool value);
        bool DisableSound();
        void DisableSound(bool value);
        event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(event_token const& token) noexcept;
    private:
        event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct SettingsViewModel : SettingsViewModelT<SettingsViewModel, implementation::SettingsViewModel>
    {
    };
}