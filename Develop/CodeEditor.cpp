#include "pch.h"
#include "CodeEditor.h"
#if __has_include("CodeEditor.g.cpp")
#include "CodeEditor.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Storage;
using namespace Windows::UI::Text;
using namespace Windows::UI::Xaml;
using namespace Windows::Foundation;

namespace winrt::Develop::implementation
{
    CodeEditor::CodeEditor(Windows::Storage::StorageFile const& file) : m_WorkingFile(file)
    {
        InitializeComponent();
    }

    fire_and_forget CodeEditor::LoadFileAsync()
    {
        apartment_context uiThread;
        bool isRichText = IsRichText();
        m_EditorHelper = TextEditor::UI::SyntaxEditor(Editor(), isRichText);
        if (isRichText)
        {
            auto stream = co_await m_WorkingFile.OpenReadAsync();
            Editor().TextDocument().LoadFromStream(TextSetOptions::FormatRtf, stream);
        }
        else
        {
            co_await resume_background();
            auto readBuffer = co_await FileIO::ReadBufferAsync(m_WorkingFile);
            m_FileReadData = JUVStudios::EncodingDetect::DetectPlusGet(readBuffer);
            co_await uiThread;
            m_EditorHelper.Text(m_FileReadData->Text);
        }
    }

    StorageFile CodeEditor::WorkingFile() { return m_WorkingFile; }

    IAsyncOperation<bool> CodeEditor::CloseAsync()
    {
        return Windows::Foundation::IAsyncOperation<bool>();
    }

    void CodeEditor::UserControl_Loaded(IInspectable const& sender, RoutedEventArgs const& e)
    {
        if (m_EditorHelper == nullptr) LoadFileAsync();
    }

    event_token CodeEditor::PropertyChanged(Data::PropertyChangedEventHandler const& handler) { return m_PropertyChanged.add(handler); }

    void CodeEditor::PropertyChanged(winrt::event_token const& token) noexcept { m_PropertyChanged.remove(token); }
}