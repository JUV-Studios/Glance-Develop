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

    void SettingsView::TwoPaneView_ModeChanged(Microsoft::UI::Xaml::Controls::TwoPaneView const& sender, IInspectable const&)
    {
        if (sender.Mode() == Microsoft::UI::Xaml::Controls::TwoPaneViewMode::Tall) sender.Pane1Length(GridLengthHelper::Auto());
        else sender.Pane1Length(GridLengthHelper::FromPixels(320));
    }

    void SettingsView::GeneralItem_Loaded(IInspectable const& sender, RoutedEventArgs const&)
    {
        auto target = sender.as<Controls::ListBoxItem>();
        target.Content(box_value(JUVStudios::ResourceController::GetTranslation(L"Settings_General/Text")));
    }

    void SettingsView::EditorOption_Loaded(IInspectable const& sender, RoutedEventArgs const&)
    {
        auto target = sender.as<Controls::ListBoxItem>();
        target.Content(box_value(JUVStudios::ResourceController::GetTranslation(L"Settings_Editor/Text")));
    }
}