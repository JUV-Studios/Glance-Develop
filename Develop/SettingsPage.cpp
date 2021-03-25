#include "pch.h"
#include "SettingsPage.h"
#if __has_include("SettingsPage.g.cpp")
#include "SettingsPage.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Automation;
using namespace Windows::Foundation;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Resources;

namespace winrt::Develop::implementation
{
    SettingsPage::SettingsPage()
    {
        InitializeComponent();
    }

    hstring SettingsPage::AboutText()
    {
        if (m_AboutText.empty())
        {
            auto resourceLoader = ResourceLoader::GetForCurrentView();
            auto packageVersion = Package::Current().Id().Version();
            std::wstringstream aboutTextStream;
            aboutTextStream << L"Develop\n" << resourceLoader.GetString(L"VersionText").data() << L" ";
            aboutTextStream << packageVersion.Major << L"." << packageVersion.Minor << L"." << packageVersion.Build << L"." << packageVersion.Revision << "\n";
            aboutTextStream << resourceLoader.GetString(L"CopyrightBlock/Text").data() << L"\n" << resourceLoader.GetString(L"DevelopedBlock/Text").data() << "\n";
            aboutTextStream << resourceLoader.GetString(L"BuiltOnBlock/Text").data() << L" " << TEXT(__TIMESTAMP__);
            m_AboutText = aboutTextStream.str();
        }

        return m_AboutText;
    }

    void SettingsPage::AboutStackPanel_Loaded(IInspectable const& sender, RoutedEventArgs const&)
    {
        auto target = sender.as<StackPanel>();
        if (AutomationProperties::GetName(target).empty())
        {
            hstring lineEndingString = L"\n";
            hstring commaSeparator = L", ";
            hstring result;
            check_hresult(WindowsReplaceString(static_cast<HSTRING>(get_abi(m_AboutText)), static_cast<HSTRING>(get_abi(lineEndingString)), 
                static_cast<HSTRING>(get_abi(commaSeparator)), reinterpret_cast<HSTRING*>(put_abi(result))));
            AutomationProperties::SetName(target, ResourceLoader::GetForCurrentView().GetString(L"Settings_About/Header") + L", " + result);
        }
    }
}