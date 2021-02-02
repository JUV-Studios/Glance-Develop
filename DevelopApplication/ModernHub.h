#pragma once

#include "ModernHub.g.h"

namespace winrt::Develop::implementation
{
    struct ModernHub : ModernHubT<ModernHub>
    {
        ModernHub();
        void Hub_SizeChanged(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::SizeChangedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ModernHub : ModernHubT<ModernHub, implementation::ModernHub>
    {
    };
}
