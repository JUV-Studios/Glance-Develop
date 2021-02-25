#pragma once

#include "AppSettings.g.h"
#include <winrt/JUVStudios.h>

namespace winrt::Develop::implementation
{
    struct AppSettings : AppSettingsT<AppSettings>
    {
        AppSettings() = delete;
        static Windows::Foundation::Collections::IVectorView<hstring> SupportedFileTypes();
        static Windows::Foundation::IAsyncAction InitializeAsync();
        static bool DialogShown();
        static void DialogShown(bool value);
        static void AddToCloseList(Develop::IAsyncClosable const& view);
        static void RemoveFromCloseList(Develop::IAsyncClosable const& view);
        static bool IsFileTypeSupported(hstring const& fileType);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct AppSettings : AppSettingsT<AppSettings, implementation::AppSettings>
    {
    };
}