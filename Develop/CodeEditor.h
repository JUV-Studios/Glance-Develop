#pragma once
#include "CodeEditor.g.h"
#include <winrt/TextEditor.Utils.h>

namespace winrt::Develop::implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor>
    {
    private:
        const Windows::Storage::StorageFile m_WorkingFile { nullptr };
        event_token m_HotKeyToken;
        event_token m_ShareRequestToken;
        event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
        bool m_FileSaveLock = false;
        bool m_Saved = true;
        bool m_Unloaded = true;
        void KeyPressHandler(Windows::UI::Core::CoreDispatcher const& dispatcher, Windows::UI::Core::AcceleratorKeyEventArgs const& e);
        void Editor_ShareRequested(Windows::ApplicationModel::DataTransfer::DataTransferManager const& sender, Windows::ApplicationModel::DataTransfer::DataRequestedEventArgs const& e);
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
        void Share_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Windows::UI::Xaml::RoutedEventArgs const& e);
        void StandardCommand_Loaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void TextView_ContentChanged(bool isReset);
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