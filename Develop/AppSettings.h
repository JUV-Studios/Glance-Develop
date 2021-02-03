#pragma once

#include "AppSettings.g.h"

namespace winrt::Develop::implementation
{
    struct AppSettings : AppSettingsT<AppSettings>
    {
        AppSettings() = delete;
        static Windows::Foundation::Collections::IVectorView<hstring> SupportedFileTypes();
        static Windows::Foundation::IAsyncAction InitializeAsync();
        static DevelopManaged::SettingsViewModel Preferences();
        static hstring GetLocalized(hstring const& key);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct AppSettings : AppSettingsT<AppSettings, implementation::AppSettings>
    {
    };
}
