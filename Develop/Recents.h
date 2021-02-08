#pragma once
#include "Recents.g.h"
#include <winrt\DevelopManaged.h>

namespace winrt::Develop::implementation
{
    struct Recents : RecentsT<Recents>
    {
        Recents() = delete;
        static Windows::Foundation::Collections::IObservableVector<DevelopManaged::RecentItem> RecentItems();
        static Windows::Foundation::IAsyncAction LoadRecentsAsync();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct Recents : RecentsT<Recents, implementation::Recents>
    {
    };
}
