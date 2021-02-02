#pragma once

#include "HomePage.g.h"

namespace winrt::Develop::implementation
{
    struct HomePage : HomePageT<HomePage>
    {
        HomePage();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct HomePage : HomePageT<HomePage, implementation::HomePage>
    {
    };
}
