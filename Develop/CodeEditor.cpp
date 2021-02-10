#include "pch.h"
#include "CodeEditor.h"
#if __has_include("CodeEditor.g.cpp")
#include "CodeEditor.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::UI::Text;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;

namespace winrt::Develop::implementation
{
	CodeEditor::CodeEditor(StorageFile const& file) : m_WorkingFile(file)
	{
		InitializeComponent();
	}

	void CodeEditor::AfterUndoRedoInvoked(bool lastElem, hstring const& text, int index)
	{
		TextView().Focus(FocusState::Keyboard);
		if (lastElem)
		{
			TextView().Text(m_FileReadData->Text);
			TextView().TextDocument().Selection().StartPosition(0);
		}
		else
		{
			TextView().Text(text);
			TextView().TextDocument().Selection().StartPosition(index);
			TextView().TextDocument().Selection().EndPosition(index - 1);
			TextView().TextDocument().Selection().Collapse(false);
		}
	}

	bool CodeEditor::TextChanged(hstring const& text, RoutedEventArgs const&)
	{
		if (m_FileReadData)
		{
			auto valTrim = DevelopManaged::General::TrimEndings(text);
			if (valTrim == DevelopManaged::General::TrimEndings(m_FileReadData->Text))
			{
				Saved(true);
				TextView().ClearHistory();
				return false;
			}
			else
			{
				if (TextView().CanUndo())
				{
					if (valTrim != TextView().PreviousText())
					{
						InvalidateState();
						return true;
					}
				}
				else
				{
					InvalidateState();
					return true;
				}
			}
		}

		return false;
	}

	void CodeEditor::InvalidateState()
	{
		Saved(false);
	}

	fire_and_forget CodeEditor::LoadFileAsync()
	{
		apartment_context uiThread;
		bool isRichText = m_WorkingFile.FileType() == L".rtf" || m_WorkingFile.FileType() == L".RTF";
		TextView().Initialize(isRichText);
		if (isRichText)
		{
			auto stream = co_await m_WorkingFile.OpenReadAsync();
			TextView().TextDocument().LoadFromStream(TextSetOptions::FormatRtf, stream);
		}
		else
		{
			co_await resume_background();
			auto readBuffer = co_await FileIO::ReadBufferAsync(m_WorkingFile);
			m_FileReadData = JUVStudios::EncodingDetect::DetectPlusGet(readBuffer);
			co_await uiThread;
			TextView().HistoryDone({ this, &CodeEditor::AfterUndoRedoInvoked });
			TextView().Text(m_FileReadData->Text);
		}

		TextView().TextChangeDelegate({ this, &CodeEditor::TextChanged });
		Ring().IsActive(false);
		TextView().Visibility(Visibility::Visible);
	}

	StorageFile CodeEditor::WorkingFile() { return m_WorkingFile; }

	IAsyncOperation<bool> CodeEditor::CloseAsync()
	{
		return Windows::Foundation::IAsyncOperation<bool>();
	}

	void CodeEditor::UserControl_Loaded(IInspectable const&, RoutedEventArgs const&)
	{
		if (Ring().IsActive()) LoadFileAsync();
	}
}