#include "pch.h"
#include "SettingsPage.h"
#if __has_include("SettingsPage.g.cpp")
#include "SettingsPage.g.cpp"
#endif

using namespace winrt;
using namespace Windows::UI::Xaml;

namespace winrt::Develop::implementation
{
    SettingsPage::SettingsPage()
    {
        InitializeComponent();
    }
}
