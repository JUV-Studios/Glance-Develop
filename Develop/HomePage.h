#pragma once

#include "HomePage.g.h"

namespace winrt::Develop::implementation
{
    struct HomePage : HomePageT<HomePage>
    {
    private:
        void NavigateToPage(Microsoft::UI::Xaml::Controls::NavigationViewItemBase const& item);
    public:
        HomePage();
        void NavigationView_ItemInvoked(Microsoft::UI::Xaml::Controls::NavigationView const& sender, Microsoft::UI::Xaml::Controls::NavigationViewItemInvokedEventArgs const& args);
        void SetPageIndex(uint32_t index);
        void UserControl_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct HomePage : HomePageT<HomePage, implementation::HomePage>
    {
    };
}
