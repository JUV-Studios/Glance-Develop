 #pragma once

#include "ContextableListView.g.h"

namespace winrt::Develop::implementation
{
    struct ContextableListView : ContextableListViewT<ContextableListView>
    {
        ContextableListView();
        Windows::UI::Xaml::Controls::MenuFlyout ItemContextFlyout();
        void ItemContextFlyout(Windows::UI::Xaml::Controls::MenuFlyout const& value);
        ContextableListViewFlyoutRequest RequestFunc();
        void RequestFunc(ContextableListViewFlyoutRequest const& value);
    private:
        void ListView_ContextRequested(Windows::UI::Xaml::UIElement const& sender, Windows::UI::Xaml::Input::ContextRequestedEventArgs const& args);
        ContextableListViewFlyoutRequest m_RequestFunc;
        Windows::UI::Xaml::Controls::MenuFlyout m_ItemContextFlyout;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ContextableListView : ContextableListViewT<ContextableListView, implementation::ContextableListView>
    {
    };
}
