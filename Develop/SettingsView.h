#pragma once

#include "SettingsView.g.h"

namespace winrt::Develop::implementation
{
    struct SettingsView : SettingsViewT<SettingsView>
    {
        SettingsView();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct SettingsView : SettingsViewT<SettingsView, implementation::SettingsView>
    {
    };
}
