#pragma once
#include "StartPage.g.h"

namespace winrt::Develop::implementation
{
    struct StartPage : StartPageT<StartPage>
    {
    private:
        void Navigate(Microsoft::UI::Xaml::Controls::NavigationViewItemBase const& navItem);
    public:
        StartPage();
        void NavigationView_ItemInvoked(Microsoft::UI::Xaml::Controls::NavigationView const& sender, Microsoft::UI::Xaml::Controls::NavigationViewItemInvokedEventArgs const& args);
        void UserControl_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct StartPage : StartPageT<StartPage, implementation::StartPage>
    {
    };
}
