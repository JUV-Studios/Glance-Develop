#pragma once
#include "pch.h"
#include "CodeEditor.g.h"
#include <winrt/TextEditor.UI.h>
#include <winrt/JUVStudios.h>

namespace winrt::Develop::implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor>
    {
    private:
        Windows::Storage::StorageFile m_WorkingFile;
        std::optional<JUVStudios::DetectedData> m_FileReadData;
        TextEditor::UI::SyntaxEditor m_EditorHelper{ nullptr };
        event<winrt::Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
        inline bool IsRichText() const noexcept
        {
            return m_WorkingFile.FileType() == L".rtf" || m_WorkingFile.FileType() == L".RTF";
        }

        fire_and_forget LoadFileAsync();
    public:
        CodeEditor(Windows::Storage::StorageFile const& file);
        Windows::Storage::StorageFile WorkingFile();
        Windows::Foundation::IAsyncOperation<bool> CloseAsync();
        winrt::event_token PropertyChanged(winrt::Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(winrt::event_token const& token) noexcept;
        void UserControl_Loaded(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
    };
}

namespace winrt::Develop::factory_implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor, implementation::CodeEditor>
    {
    };
}
