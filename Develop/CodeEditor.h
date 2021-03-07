#pragma once
#include "CodeEditor.g.h"

namespace winrt::Develop::implementation
{
    enum class LineEnding : uint8_t { CR, LF, CRLF, Unknown };

    struct FileReadData
    {
        Windows::Security::Cryptography::BinaryStringEncoding StringEncoding;
        LineEnding StringLineEnding;
        hstring Text;
    };

    struct CodeEditor : CodeEditorT<CodeEditor>, JUVStudios::MVVM::ViewModelBase
    {
    private:
        FileReadData m_FileReadData;
        Windows::Storage::StorageFile m_WorkingFile { nullptr };
        bool m_Unloaded = true;
        std::optional<event_token> m_KeyPressHandlerToken;
        void AfterUndoRedoInvoked(bool lastElem, hstring const& text, int index);
        bool TextChanged(hstring const& text, Windows::UI::Xaml::RoutedEventArgs const& args);
        void KeyPressHandler(Windows::UI::Core::CoreDispatcher const& dispatcher, Windows::UI::Core::AcceleratorKeyEventArgs const& e);
        fire_and_forget LoadFileAsync();
    protected:
        Windows::Foundation::IInspectable GetHolder() const noexcept override;
    public:
        CodeEditor(Windows::Storage::StorageFile const& file);
        bool StartClosing();
        Windows::Foundation::IAsyncAction CloseAsync();
        Windows::Foundation::IAsyncAction SaveFile_Click(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        JUVStudios::MVVM::ObservableProperty<bool> Saved { this, L"Saved", true };
        bool FileLoaded();
        void FileLoaded(bool value);
        void UserControl_Loaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void UserControl_Unloaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        void StandardCommand_Loaded(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& e);
        ~CodeEditor();
    };
}

namespace winrt::Develop::factory_implementation
{
    struct CodeEditor : CodeEditorT<CodeEditor, implementation::CodeEditor>
    {
    };
}