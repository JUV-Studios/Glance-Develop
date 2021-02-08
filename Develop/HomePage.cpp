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

    void HomePage::NavigationView_ItemInvoked(NavigationView const& sender, NavigationViewItemInvokedEventArgs const& args)
    {
        auto frame = sender.Content().as<Frame>();
        if (args.IsSettingsInvoked()) frame.Navigate(xaml_typename<SettingsView>(), nullptr, DrillInNavigationTransitionInfo());
    }
}