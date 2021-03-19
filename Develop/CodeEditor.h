#pragma once
#include "CodeEditor.g.h"
#include <winrt/TextEditor.Utils.h>

namespace winrt::Develop::implementation
{
    using EditorEventRegistrations = std::pair<event_token, event_token>;

    struct CodeEditor : CodeEditorT<CodeEditor>
    {
    private:
        const Windows::Storage::StorageFile m_WorkingFile { nullptr };
        std::optional<EditorEventRegistrations> m_EventRegistrations;
        std::mutex m_FileSaveLock;
        event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
        bool m_Saved = true;
        bool m_Unloaded = true;
        void KeyPressHandler(Windows::UI::Core::CoreDispatcher const& dispatcher, Windows::UI::Core::AcceleratorKeyEventArgs const& e);
        void EditorPropertyChanged(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::Data::PropertyChangedEventArgs const& args);
        fire_and_forget LoadFileAsync();
    public:
        CodeEditor(Windows::Storage::StorageFile const& file);
        bool Saved() const;
        void Saved(bool value);
        Windows::Storage::StorageFile WorkingFile();
        bool StartClosing();
        void Close();
        Windows::Foundation::IAsyncAction CloseAsync();
        Windows::Foundation::IAsyncAction SaveFile_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler) noexcept;
        void PropertyChanged(event_token token) noexcept;
        void UserControl_Loaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void UserControl_Unloaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void StandardCommand_Loaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void EditorCommand_Requested(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        ~CodeEditor();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor, implementation::CodeEditor>
    {
    };
}