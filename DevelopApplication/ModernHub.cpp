#include "pch.h"
#include "ModernHub.h"
#if __has_include("ModernHub.g.cpp")
#include "ModernHub.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::UI::Xaml;

namespace winrt::Develop::implementation
{
    ModernHub::ModernHub()
    {
        InitializeComponent();
    }

    void ModernHub::Hub_SizeChanged(IInspectable const&, SizeChangedEventArgs const& e)
    {
        if (e.NewSize().Width < 641) Margin(unbox_value<Thickness>(Resources().Lookup(box_value(L"SmallMargin"))));
        else if (e.NewSize().Width < 1008) Margin(unbox_value<Thickness>(Resources().Lookup(box_value(L"MediumMargin"))));
        else Margin(unbox_value<Thickness>(Resources().Lookup(box_value(L"LargeMargin"))));
    }
}