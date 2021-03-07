#include "CodeEditor.h"
#if __has_include("CodeEditor.g.cpp")
#include "CodeEditor.g.cpp"
#endif
#include "text_encoding_detect.h"

using namespace winrt;
using namespace JUVStudios;
using namespace AutoIt::Common;
using namespace Windows::Foundation;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::Storage::Streams;
using namespace Windows::UI::Core;
using namespace Windows::UI::Text;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::Security::Cryptography;
using namespace Windows::ApplicationModel::Core;

namespace winrt::Develop::implementation
{
	TextEncodingDetect detect;

	inline BinaryStringEncoding DetermineStringEncoding(IBuffer const& buffer)
	{
		auto encoding = detect.DetectEncoding(buffer.data(), buffer.Length());
		if (encoding == TextEncodingDetect::UTF16_BE_BOM || encoding == TextEncodingDetect::UTF16_BE_NOBOM) return BinaryStringEncoding::Utf16BE;
		else if (encoding == TextEncodingDetect::UTF16_LE_BOM || encoding == TextEncodingDetect::UTF16_LE_NOBOM) return BinaryStringEncoding::Utf16LE;
		else return BinaryStringEncoding::Utf8;
	}

	inline LineEnding DetermineLineEnding(std::wstring_view text)
	{
		auto result = LineEnding::Unknown;
		for (auto i : text)
		{
			switch (i)
			{
			case L'\r':
				result = LineEnding::CR;
				break;
			case L'\n':
				result = result == LineEnding::CR ? LineEnding::CRLF : LineEnding::LF;
				break;
			default:
				if (result != LineEnding::Unknown) result = LineEnding::Unknown;
				break;
			}
		}

		return result;
	}

	CodeEditor::CodeEditor(StorageFile const& file)
	{
		m_WorkingFile = file;
		InitializeComponent();
	}

	void CodeEditor::AfterUndoRedoInvoked(bool lastElem, hstring const& text, int index)
	{
		TextView().Focus(FocusState::Keyboard);
		if (lastElem)
		{
			TextView().Text(m_FileReadData.Text);
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
		bool acceptChanges = false;
		acceptChanges = false;
		auto value = Helpers::StringTrimEnd(text, Helpers::SpaceCharacters());
		if (value == m_FileReadData.Text)
		{
			if (TextView().CanUndo() || TextView().CanRedo())
			{
				SaveFile_Click(nullptr, nullptr);
				TextView().ClearHistory();
			}

			Saved(true);
			acceptChanges = false;
		}
		else if (TextView().CanUndo() && value == TextView().PreviousText()) acceptChanges = false;
		else if (SettingsViewModel::Instance().AutoSave())
		{
			SaveFile_Click(nullptr, nullptr);
			acceptChanges = true;
		}
		else
		{
			Saved(false);
			acceptChanges = true;
		}

		return acceptChanges;
	}

	fire_and_forget CodeEditor::LoadFileAsync()
	{
		bool isRichText = m_WorkingFile.FileType() == L".rtf" || m_WorkingFile.FileType() == L".RTF";
		co_await TextView().InitializeAsync(isRichText);
		if (isRichText)
		{
			auto stream = co_await m_WorkingFile.OpenReadAsync();
			TextView().TextDocument().LoadFromStream(TextSetOptions::FormatRtf, stream);
		}
		else
		{
			auto readBuffer = co_await FileIO::ReadBufferAsync(m_WorkingFile);
			m_FileReadData.StringEncoding = DetermineStringEncoding(readBuffer);
			m_FileReadData.Text = Helpers::StringTrimEnd(Helpers::StringReplaceText(CryptographicBuffer::ConvertBinaryToString(m_FileReadData.StringEncoding, readBuffer), 
				L"\uFEFF", L""), Helpers::SpaceCharacters());
			m_FileReadData.StringLineEnding = DetermineLineEnding(m_FileReadData.Text);
			TextView().Text(m_FileReadData.Text);
			EncodingBox().SelectedIndex(static_cast<int>(m_FileReadData.StringEncoding));
		}

		FileLoaded(true);
	}

	IInspectable CodeEditor::GetHolder() const noexcept { return *this; }

	IAsyncAction CodeEditor::CloseAsync()
	{
		if (!Saved()) co_await SaveFile_Click(nullptr, nullptr);
		FileLoaded(false);
	}

	bool CodeEditor::FileLoaded()
	{
		return m_KeyPressHandlerToken.has_value();
	}

	void CodeEditor::FileLoaded(bool value)
	{
		if (value != FileLoaded())
		{
			if (value)
			{
				// Attach event handlers
				m_KeyPressHandlerToken = CoreApplication::GetCurrentView().CoreWindow().Dispatcher().AcceleratorKeyActivated({ this, &CodeEditor::KeyPressHandler });
				TextView().TextChangeDelegate({ this, &CodeEditor::TextChanged });
				TextView().HistoryDone({ this, &CodeEditor::AfterUndoRedoInvoked });
				Content().Visibility(Visibility::Visible);
			}
			else
			{
				// Remove event handlers
				if (m_KeyPressHandlerToken)
				{
					CoreApplication::GetCurrentView().CoreWindow().Dispatcher().AcceleratorKeyActivated(m_KeyPressHandlerToken.value());
					m_KeyPressHandlerToken.reset();
				}

				m_WorkingFile = nullptr;
				Content().Visibility(Visibility::Collapsed);
				TextView().TextChangeDelegate(nullptr);
				TextView().HistoryDone(nullptr);
				TextView().Close();
			}

			RaisePropertyChanged(L"FileLoaded");
		}
	}

	bool CodeEditor::StartClosing()
	{
		if (FileLoaded())
		{
			Bindings->StopTracking();
			return true;
		}
		else return false;
	}

	void CodeEditor::KeyPressHandler(Windows::UI::Core::CoreDispatcher const&, AcceleratorKeyEventArgs const& e)
	{
		if (!AppSettings::DialogShown() && !m_Unloaded && e.EventType() == CoreAcceleratorKeyEventType::KeyDown && IsInKeyDown(VirtualKey::Control))
		{
			switch (e.VirtualKey())
			{
			case VirtualKey::Z:
				// Handle Ctrl+Z
				e.Handled(true);
				TextView().Undo();
				break;
				
			case VirtualKey::Y:
				// Handle Ctrl+Y
				e.Handled(true);
				TextView().Redo();
				break;

			case VirtualKey::S:
				// Handle Ctrl+S
				e.Handled(true);
				SaveFile_Click(nullptr, nullptr);
				break;

			default:
				// Don't handle the 
				e.Handled(false);
				break;
			}
		}
	}

	void CodeEditor::UserControl_Loaded(IInspectable const&, RoutedEventArgs const&)
	{
		m_Unloaded = false;
		if (!FileLoaded()) LoadFileAsync();
	}

	void CodeEditor::UserControl_Unloaded(IInspectable const&, RoutedEventArgs const&)
	{
		m_Unloaded = true;
	}

	void CodeEditor::StandardCommand_Loaded(IInspectable const& sender, RoutedEventArgs const&)
	{
		auto target = sender.as<AppBarButton>();
		auto tag = unbox_value<hstring>(target.Tag());
		StandardUICommand command;
		if (tag == L"Undo") command.Kind(StandardUICommandKind::Undo);
		else if (tag == L"Redo") command.Kind(StandardUICommandKind::Redo);
		else if (tag == L"SelectAll") command.Kind(StandardUICommandKind::SelectAll);
		else if (tag == L"Save") command.Kind(StandardUICommandKind::Save);

		if (target.Label().empty())
		{
			IconSourceElement iconElement;
			iconElement.IconSource(command.IconSource());
			target.Label(command.Label());
			target.Icon(iconElement);
			target.AccessKey(command.AccessKey());
			ToolTipService::SetToolTip(target, box_value(command.Description()));
		}
	}

	CodeEditor::~CodeEditor()
	{
		DebugBreak();
	}

	IAsyncAction CodeEditor::SaveFile_Click(IInspectable const&, RoutedEventArgs const&)
	{
		if (FileLoaded())
		{
			if (!co_await TextView().WriteFileAsync(m_WorkingFile, static_cast<BinaryStringEncoding>(EncodingBox().SelectedIndex())))
			{
				// TODO Work on recovery system
			}
			else Saved(true);
		}
	}
}