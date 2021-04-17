#include "pch.h"
#include "MainPage.h"
#include "MainPage.g.cpp"

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::ApplicationModel::Core;

namespace winrt::Develop::implementation
{
    MainPage::MainPage()
    {
        InitializeComponent();
    }

    void MainPage::Page_Loaded(IInspectable const&, RoutedEventArgs const&)
    {
        if (!NavView().SelectedItem())
        {
            NavView().SelectedItem(NavView().MenuItems().GetAt(0));
        }
    }

    void MainPage::FlipView_SelectionChanged(IInspectable const&, SelectionChangedEventArgs const&)
    {
        auto selectedIndex = static_cast<uint32_t>(FlipView().SelectedIndex());
        auto navMenuItems = NavView().MenuItems();
        if (selectedIndex == navMenuItems.Size())
        {
            NavView().SelectedItem(NavView().SettingsItem());
        }
        else
        {
            NavView().SelectedItem(navMenuItems.GetAt(selectedIndex));
        }
    }

    void MainPage::NavigationView_ItemInvoked(Microsoft::UI::Xaml::Controls::NavigationView const&, Microsoft::UI::Xaml::Controls::NavigationViewItemInvokedEventArgs const& args)
    {
        if (args.IsSettingsInvoked())
        {
            FlipView().SelectedIndex(NavView().MenuItems().Size());
        }
        else
        {
            uint32_t index;
            NavView().MenuItems().IndexOf(args.InvokedItem(), index);
            FlipView().SelectedIndex(index);
        }
    }

    bool MainPage::IsViewExtended() const noexcept
    {
        return CoreApplication::GetCurrentView().TitleBar().ExtendViewIntoTitleBar();
    }

    void MainPage::IsViewExtended(bool value)
    {
        CoreApplication::GetCurrentView().TitleBar().ExtendViewIntoTitleBar(value);
        if (value)
        {
            Window::Current().SetTitleBar(DragRegion());
        }
        else
        {
            Window::Current().SetTitleBar(nullptr);
        }
    }
}