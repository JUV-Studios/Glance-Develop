#pragma once
#include "ShellView.g.h"

namespace winrt::Develop::implementation
{
    struct ShellView : ShellViewT<ShellView>
    {
        ShellView() = default;
        ShellView(hstring const& title, Windows::UI::Xaml::UIElement const& content, Windows::UI::Xaml::Controls::IconSource const& icon, Windows::Storage::IStorageItem2 const& refSource);
        hstring Caption();
        Windows::UI::Xaml::UIElement Content();
        Windows::UI::Xaml::Controls::IconSource Icon();
        Windows::Storage::IStorageItem2 ReferenceSource();
        bool CanClose();
        void Close();
        hstring ToString();
        ~ShellView();
    private:
        hstring m_Title;
        Windows::UI::Xaml::UIElement m_Content;
        Windows::UI::Xaml::Controls::IconSource m_IconSource;
        Windows::Storage::IStorageItem2 m_ReferenceSource;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellView : ShellViewT<ShellView, implementation::ShellView>
    {
    };
}
