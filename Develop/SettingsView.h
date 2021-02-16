#pragma once
#include "SettingsView.g.h"
#include "App.h"

namespace winrt::Develop::implementation
{
    struct SettingsView : SettingsViewT<SettingsView>
    {
    private:
        JUVStudios::BindableObject m_Bindable{ *this };
    public:
        SettingsView();
        ObservablePrimitiveProperty(int, SelectedPage, m_Bindable);
        PropertyChangedHandler(m_Bindable);
        void TwoPaneView_ModeChanged(winrt::Microsoft::UI::Xaml::Controls::TwoPaneView const& sender, winrt::Windows::Foundation::IInspectable const& args);
        void GeneralItem_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void EditorOption_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct SettingsView : SettingsViewT<SettingsView, implementation::SettingsView>
    {
    };
}
