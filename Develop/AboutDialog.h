#pragma once
#include "AboutDialog.g.h"

namespace winrt::Develop::implementation
{
    struct AboutDialog : AboutDialogT<AboutDialog>
    {
        AboutDialog();
        static Windows::Foundation::IAsyncAction ShowDialogAsync();
        void BuiltonBlock_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        hstring OkayTextId();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct AboutDialog : AboutDialogT<AboutDialog, implementation::AboutDialog>
    {
    };
}