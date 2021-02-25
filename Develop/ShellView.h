#pragma once
#include "ShellView.g.h"

namespace winrt::Develop::implementation
{
    struct ShellView : ShellViewT<ShellView>
    {
        ShellView() = default;
        ShellView(hstring const& title, Windows::UI::Xaml::UIElement const& content, Windows::UI::Xaml::Controls::FontIconSource const& icon, Windows::Storage::IStorageItem2 const& refSource);
        hstring Caption();
        Windows::UI::Xaml::UIElement Content();
        Windows::UI::Xaml::Controls::FontIconSource Icon();
        Windows::Storage::IStorageItem2 ReferenceSource();
        bool CanClose();
        hstring ToString();
    private:
        hstring m_Title;
        hstring m_Path;
        Windows::UI::Xaml::UIElement m_Content;
        Windows::UI::Xaml::Controls::FontIconSource m_IconSource;
        Windows::Storage::IStorageItem2 m_ReferenceSource;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellView : ShellViewT<ShellView, implementation::ShellView>
    {
    };
}
