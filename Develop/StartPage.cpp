#include "pch.h"
#include "StartPage.h"
#if __has_include("StartPage.g.cpp")
#include "StartPage.g.cpp"
#endif

using namespace winrt;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Interop;
using namespace Windows::Foundation;

namespace winrt::Develop::implementation
{
    const hstring SettingsTypeName = L"Develop.SettingsPage";

    void StartPage::Navigate(Microsoft::UI::Xaml::Controls::NavigationViewItemBase const& navItem)
    {
        TypeName pageTypeName;
        pageTypeName.Name = navItem == Navigation().SettingsItem() ? SettingsTypeName : unbox_value<hstring>(navItem.Tag());
        pageTypeName.Kind = Windows::UI::Xaml::Interop::TypeKind::Metadata;
        ContentFrame().Navigate(pageTypeName, nullptr, Windows::UI::Xaml::Media::Animation::DrillInNavigationTransitionInfo());
        Navigation().SelectedItem(navItem);
    }

    StartPage::StartPage()
    {
        InitializeComponent();
    }

    void StartPage::NavigationView_ItemInvoked(Microsoft::UI::Xaml::Controls::NavigationView const&, Microsoft::UI::Xaml::Controls::NavigationViewItemInvokedEventArgs const& args)
    {
        if (args.InvokedItemContainer().as<Microsoft::UI::Xaml::Controls::NavigationViewItem>().SelectsOnInvoked()) Navigate(args.InvokedItemContainer());
        else return;
    }

    void StartPage::UserControl_Loaded(IInspectable const&, RoutedEventArgs const&)
    {
        if (Navigation().SelectedItem() == nullptr) Navigate(Navigation().MenuItems().GetAt(0).as<Microsoft::UI::Xaml::Controls::NavigationViewItemBase>());
    }
}
