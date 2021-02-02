#include "pch.h"
#include "ModernHub.h"
#if __has_include("ModernHub.g.cpp")
#include "ModernHub.g.cpp"
#endif

using namespace winrt;
using namespace Windows::UI::Xaml;
using namespace Windows::Foundation;

namespace winrt::Develop::implementation
{
    ModernHub::ModernHub()
    {
        InitializeComponent();
    }

    hstring ModernHub::Header() { return HeadingBlock().Text(); }

    void ModernHub::Header(hstring const& value) { HeadingBlock().Text(value); }

    void ModernHub::Layout_SizeChanged(IInspectable const& sender, SizeChangedEventArgs const& e)
    {

            if (e.NewSize().Width < 641) Margin(ThicknessHelper::FromLengths(24, 24, 24, 0));
            else if (e.NewSize().Width < 1008) Margin(ThicknessHelper::FromLengths(36, 36, 36, 0));
            else Margin(ThicknessHelper::FromLengths(48, 48, 48, 0));
    }
}