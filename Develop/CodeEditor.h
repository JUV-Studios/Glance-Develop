#pragma once
#include "pch.h"
#include "CodeEditor.g.h"
#include <winrt/TextEditor.UI.h>
#include <winrt/DevelopManaged.h>
#include <winrt/JUVStudios.h>

namespace winrt::Develop::implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor>
    {
    private:
        Windows::Storage::StorageFile m_WorkingFile;
        std::optional<JUVStudios::DetectedData> m_FileReadData;
        JUVStudios::BindableObject m_Bindable{ *this };
        void AfterUndoRedoInvoked(bool lastElem, hstring const& text, int index);
        bool TextChanged(hstring const& text, Windows::UI::Xaml::RoutedEventArgs const& args);
        void InvalidateState();
        fire_and_forget LoadFileAsync();
    public:
        CodeEditor(Windows::Storage::StorageFile const& file);
        Windows::Storage::StorageFile WorkingFile();
        Windows::Foundation::IAsyncOperation<bool> CloseAsync();
        bool Saved() const { return unbox_value<bool>(m_Bindable.GetProperty(L"Saved", box_value(true))); }
        void Saved(bool value) { m_Bindable.SetProperty(L"Saved", box_value(value)); }
        inline TextEditor::UI::SyntaxEditor Editor() { return TextView(); }
        inline event_token PropertyChanged(winrt::Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler) const { return m_Bindable.PropertyChanged(handler); }
        inline void PropertyChanged(winrt::event_token const& token) noexcept { m_Bindable.PropertyChanged(token); }
        void UserControl_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor, implementation::CodeEditor>
    {
    };
}
