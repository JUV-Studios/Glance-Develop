#pragma once
#include "SettingsViewModel.g.h"

namespace winrt::Develop::implementation
{
    struct SettingsViewModel : SettingsViewModelT<SettingsViewModel>, JUVStudios::MVVM::ViewModelBase
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
    protected:
        Windows::Foundation::IInspectable GetHolder() const noexcept override;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct SettingsViewModel : SettingsViewModelT<SettingsViewModel, implementation::SettingsViewModel>
    {
    };
}