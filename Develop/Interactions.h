#pragma once
#include "Interactions.g.h"

namespace winrt::Develop::implementation
{
    struct Interactions : InteractionsT<Interactions>
    {
        Interactions() = delete;
        static Windows::Foundation::IAsyncAction OpenFile();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct Interactions : InteractionsT<Interactions, implementation::Interactions>
    {
    };
}