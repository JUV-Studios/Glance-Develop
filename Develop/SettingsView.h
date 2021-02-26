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
        hstring GeneralHeaderId();
        hstring EditorOptionsHeaderId();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct SettingsView : SettingsViewT<SettingsView, implementation::SettingsView>
    {
    };
}
