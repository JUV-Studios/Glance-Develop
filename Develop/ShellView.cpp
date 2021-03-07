#include "ShellView.h"
#if __has_include("ShellView.g.cpp")
#include "ShellView.g.cpp"
#endif
#include "App.h"

using namespace winrt;
using namespace JUVStudios;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;

namespace winrt::Develop::implementation
{
    ShellView::ShellView(hstring const& title, UIElement const& content, FontIconSource const& icon, IStorageItem2 const& refSource) : m_Title(title), m_Content(content), m_IconSource(icon),
        m_ReferenceSource(refSource) {}

    hstring ShellView::Caption()
    {
        if (m_ReferenceSource == nullptr) return Helpers::GetResourceTranslation(L"WelcomeText");
        else
        {
            if (m_Path.empty()) m_Path = std::filesystem::path(m_ReferenceSource.Path().data()).parent_path().c_str();
            return m_Path;
        }
    }

    UIElement ShellView::Content() { return m_Content; }

    FontIconSource ShellView::Icon() { return m_IconSource; }

    IStorageItem2 ShellView::ReferenceSource() { return m_ReferenceSource; }

    bool ShellView::CanClose() { return m_Content.try_as<IClosable>() != nullptr; }

    hstring ShellView::ToString() { return m_Title; }
}