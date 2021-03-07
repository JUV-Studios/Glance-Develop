#include "pch.h"
#include "HomePage.h"
#if __has_include("HomePage.g.cpp")
#include "HomePage.g.cpp"
#endif

using namespace winrt;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Media::Animation;

namespace winrt::Develop::implementation
{
    HomePage::HomePage()
    {
        InitializeComponent();
    }

    void HomePage::NavigationView_ItemInvoked(Microsoft::UI::Xaml::Controls::NavigationView const& sender, Microsoft::UI::Xaml::Controls::NavigationViewItemInvokedEventArgs const& args)
    {
    }

    void HomePage::NavigateToPage(Microsoft::UI::Xaml::Controls::NavigationViewItemBase const& item)
    {
    }

    void HomePage::SetPageIndex(uint32_t index)
    {
        auto item = NavigationView().MenuItems().GetAt(index).as<Microsoft::UI::Xaml::Controls::NavigationViewItemBase>();
        NavigationView().SelectedItem(item);
        NavigateToPage(item);
    }

    void HomePage::UserControl_Loaded(IInspectable const&, RoutedEventArgs const&)
    {
        if (NavigationView().SelectedItem() == nullptr) SetPageIndex(0);
    }
}