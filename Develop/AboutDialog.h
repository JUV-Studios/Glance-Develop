#pragma once
#include "AboutDialog.g.h"

namespace winrt::Develop::implementation
{
    struct AboutDialog : AboutDialogT<AboutDialog>
    {
        AboutDialog();
        void ContentDialog_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        static Windows::Foundation::IAsyncAction ShowDialogAsync();
        void BuiltonBlock_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct AboutDialog : AboutDialogT<AboutDialog, implementation::AboutDialog>
    {
    };
}
