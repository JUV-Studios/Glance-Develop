#pragma once
#include "MainPage.g.h"
#include "AboutDialog.h"

namespace winrt::Develop::implementation
{
    struct MainPage : MainPageT<MainPage>
    {
    private:
        Windows::ApplicationModel::Core::CoreApplicationView m_AppView{ nullptr };
        Windows::Storage::Pickers::FileOpenPicker m_OpenPicker{ nullptr };
        Develop::ShellViewModel m_ViewModel;
        fire_and_forget OpenFile();
    public:
        MainPage();
        Develop::ShellViewModel ViewModel();
        void KeyPressHandler(Windows::UI::Core::CoreDispatcher const& sender, Windows::UI::Core::AcceleratorKeyEventArgs const& e);
        void AboutItem_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void AboutItem_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void AppMenu_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void OpenFile_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void OpenProject_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void CloseCurrentTab_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void JoinDiscordServer_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void JoinDiscordServer_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct MainPage : MainPageT<MainPage, implementation::MainPage>
    {
    };
}
