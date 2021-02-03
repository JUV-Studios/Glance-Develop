#pragma once

#include "MainPage.g.h"
#include "ShellViewModel.h"
#include <winrt\Develop.h>

namespace winrt::Develop::implementation
{
    struct MainPage : MainPageT<MainPage>
    {
    private:
        Windows::ApplicationModel::Core::CoreApplicationView m_AppView{ nullptr };
        Develop::ShellViewModel m_ViewModel;
    public:
        MainPage();
        Develop::ShellViewModel ViewModel();
        void UserControl_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct MainPage : MainPageT<MainPage, implementation::MainPage>
    {
    };
}
