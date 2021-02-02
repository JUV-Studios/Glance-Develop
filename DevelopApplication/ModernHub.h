#pragma once

#include "pch.h"
#include "ModernHub.g.h"

namespace winrt::Develop::implementation
{
    struct ModernHub : ModernHubT<ModernHub>
    {
        ModernHub();
        hstring Header();
        void Header(hstring const& value);
        void Layout_SizeChanged(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::SizeChangedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ModernHub : ModernHubT<ModernHub, implementation::ModernHub>
    {
    };
}
