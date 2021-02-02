#include "pch.h"
#include "MainPage.h"
#include "MainPage.g.cpp"

using namespace winrt;
using namespace Windows::UI::Xaml;
using namespace Windows::Foundation;
using namespace Windows::ApplicationModel::Core;

namespace winrt::Develop::implementation
{
    MainPage::MainPage()
    {
        InitializeComponent();
        m_AppView = CoreApplication::GetCurrentView();
        m_AppView.TitleBar().ExtendViewIntoTitleBar(true);
    }

    void MainPage::UserControl_Loaded(IInspectable const& sender, RoutedEventArgs const& e)
    {
        Window::Current().SetTitleBar(DragRegion());
    }

    Develop::ShellViewModel MainPage::ViewModel() { return m_ViewModel; }
}