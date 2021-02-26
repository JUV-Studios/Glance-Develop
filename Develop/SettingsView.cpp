#include "pch.h"
#include "SettingsView.h"
#if __has_include("SettingsView.g.cpp")
#include "SettingsView.g.cpp"
#endif

using namespace winrt;
using namespace Windows::UI::Xaml;
using namespace Windows::Foundation;

namespace winrt::Develop::implementation
{
    SettingsView::SettingsView()
    {
        InitializeComponent();
        SelectedPage(0);
    }

    hstring SettingsView::GeneralHeaderId() { return L"Settings_General/Text"; }

    hstring SettingsView::EditorOptionsHeaderId() { return L"Settings_Editor/Text"; }
}