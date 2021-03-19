#pragma once
#include "MainPage.g.h"

namespace winrt::Develop::implementation
{
    struct MainPage : MainPageT<MainPage>
    {
    private:
        void KeyPressHandler(Windows::UI::Core::CoreDispatcher const&, Windows::UI::Core::AcceleratorKeyEventArgs const& e);
    public:
        MainPage();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct MainPage : MainPageT<MainPage, implementation::MainPage>
    {
    };
}