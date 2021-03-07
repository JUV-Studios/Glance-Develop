#pragma once
#include "MainPage.g.h"
#include "AboutDialog.h"

namespace winrt::Develop::implementation
{
    struct MainPage : MainPageT<MainPage>
    {
    private:
        void KeyPressHandler(Windows::UI::Core::CoreDispatcher const&, Windows::UI::Core::AcceleratorKeyEventArgs const& e);
    public:
        MainPage();
        void GoStart();
        void GoOpen();
        void CloseCurrentTab_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct MainPage : MainPageT<MainPage, implementation::MainPage>
    {
    };
}