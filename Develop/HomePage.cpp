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
        auto frame = sender.Content().as<Frame>();
        if (args.IsSettingsInvoked()) frame.Navigate(xaml_typename<SettingsView>(), nullptr, DrillInNavigationTransitionInfo());
        else if (args.InvokedItemContainer()) NavigateToPage(args.InvokedItemContainer());
    }

    void HomePage::NavigateToPage(Microsoft::UI::Xaml::Controls::NavigationViewItemBase const& item)
    {
        Windows::UI::Xaml::Interop::TypeName pageTypeName;
        pageTypeName.Name = unbox_value<hstring>(item.Tag());
        pageTypeName.Kind = Windows::UI::Xaml::Interop::TypeKind::Primitive;
        NavigationView().Content().as<Frame>().Navigate(pageTypeName, nullptr, DrillInNavigationTransitionInfo());
    }

    void HomePage::SetPageIndex(uint32_t index)
    {
        auto item = NavigationView().MenuItems().GetAt(index).as<Microsoft::UI::Xaml::Controls::NavigationViewItemBase>();
        NavigationView().SelectedItem(item);
        NavigateToPage(item);
    }

    hstring HomePage::OpenTitleId() { return L"OpenPageTitle/Text"; }
}