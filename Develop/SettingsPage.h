#pragma once
#include "SettingsPage.g.h"

namespace winrt::Develop::implementation
{
    struct SettingsPage : SettingsPageT<SettingsPage>
    {
    private:
        hstring m_AboutText;
    public:
        SettingsPage();
        hstring AboutText();
        void AboutStackPanel_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct SettingsPage : SettingsPageT<SettingsPage, implementation::SettingsPage>
    {
    };
}
