#include "pch.h"
#include "ShellView.h"
#if __has_include("ShellView.g.cpp")
#include "ShellView.g.cpp"
#endif
#include "App.h"
#include <winstring.h>

using namespace winrt;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;

namespace winrt::Develop::implementation
{
    ShellView::ShellView(hstring const& title, UIElement const& content, IconSource const& icon, IStorageItem2 const& refSource) : m_Title(title), m_Content(content), m_IconSource(icon), 
        m_ReferenceSource(refSource) {}

    hstring ShellView::Caption()
    {
        if (m_ReferenceSource == nullptr) return L"";
        else return std::filesystem::path(m_ReferenceSource.Path().data()).parent_path().c_str();
    }

    UIElement ShellView::Content() { return m_Content; }

    IconSource ShellView::Icon() { return m_IconSource; }

    IStorageItem2 ShellView::ReferenceSource() { return m_ReferenceSource; }

    bool ShellView::CanClose() { return m_Content.try_as<IAsyncClosable>() != nullptr; }

    void ShellView::Close()
    {
        m_Title = L"";
        m_Content = nullptr;
        m_IconSource = nullptr;
        m_ReferenceSource = nullptr;
    }

    hstring ShellView::ToString() { return m_Title; }

    ShellView::~ShellView() { Close(); }
}