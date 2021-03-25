#include "ShellView.h"
#if __has_include("ShellView.g.cpp")
#include "ShellView.g.cpp"
#endif
#include "App.h"

using namespace winrt;
using namespace Shared;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::ApplicationModel::Resources;

namespace winrt::Develop::implementation
{
    ShellView::ShellView(hstring const& title, UIElement const& content, FontIconSource const& icon, IStorageItem2 const& refSource) : m_Title(title), m_Content(content), m_IconSource(icon),
        m_ReferenceSource(refSource) {}

    void ShellView::Close()
    {
        m_Title = L"";
        m_Caption = L"";
        m_Content = nullptr;
        m_IconSource = nullptr;
        m_ReferenceSource = nullptr;
    }

    hstring ShellView::Caption()
    {
        if (m_Caption.empty())
        {
            if (m_ReferenceSource == nullptr) m_Caption = ResourceLoader::GetForViewIndependentUse().GetString(L"WelcomeText");
            else m_Caption = std::filesystem::path(m_ReferenceSource.Path().data()).parent_path().c_str();
        }

        return m_Caption;
    }

    UIElement ShellView::Content() 
    {
        return m_Content;
    }

    FontIconSource ShellView::Icon() { return m_IconSource; }

    IStorageItem2 ShellView::ReferenceSource() { return m_ReferenceSource; }

    bool ShellView::CanClose()
    {
        if (m_Content == nullptr) return true;
        else return m_Content.try_as<IClosable>() != nullptr; 
    }

    hstring ShellView::ToString() { return m_Title; }
}