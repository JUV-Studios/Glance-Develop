#pragma once

#include "HomePage.g.h"

namespace winrt::Develop::implementation
{
    struct HomePage : HomePageT<HomePage>
    {
        HomePage();
        void NavigationView_ItemInvoked(winrt::Windows::UI::Xaml::Controls::NavigationView const& sender, winrt::Windows::UI::Xaml::Controls::NavigationViewItemInvokedEventArgs const& args);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct HomePage : HomePageT<HomePage, implementation::HomePage>
    {
    };
}
