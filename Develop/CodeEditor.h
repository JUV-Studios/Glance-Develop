#pragma once
#include "pch.h"
#include "CodeEditor.g.h"
#include "App.h"

namespace winrt::Develop::implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor>
    {
    private:
        std::optional<JUVStudios::DetectedData> m_FileReadData;
        JUVStudios::BindableObject m_Bindable{ *this };
        bool m_LoadSaveLock = false;
        void AfterUndoRedoInvoked(bool lastElem, hstring const& text, int index);
        bool TextChanged(hstring const& text, Windows::UI::Xaml::RoutedEventArgs const& args);
        fire_and_forget LoadFileAsync();
        Windows::Foundation::IAsyncAction SaveFileAsync();
    public:
        CodeEditor(Windows::Storage::StorageFile const& file);
        bool PrepareClose();
        Windows::Foundation::IAsyncAction CloseAsync();
        void Close();
        ObservablePrimitiveProperty(bool, Saved, m_Bindable);
        ObservableReferenceProperty(Windows::Storage::StorageFile, WorkingFile, m_Bindable);
        ObservableReferenceProperty(TextEditor::UI::SyntaxEditor, TextView, m_Bindable);
        PropertyChangedHandler(m_Bindable);
        void UserControl_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void SaveFile_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor, implementation::CodeEditor>
    {
    };
}
