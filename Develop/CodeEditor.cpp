#include "CodeEditor.h"
#if __has_include("CodeEditor.g.cpp")
#include "CodeEditor.g.cpp"
#endif

using namespace winrt;
using namespace JUVStudios;
using namespace Windows::Foundation;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::UI::Core;
using namespace Windows::UI::Text;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::ApplicationModel::Core;

namespace winrt::Develop::implementation
{
	CodeEditor::CodeEditor(StorageFile const& file)
	{
		WorkingFile(file);
		Saved(true);
		TextView(TextEditor::UI::SyntaxEditor());
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
			auto valTrim = Helpers::TrimEnd(text, Helpers::SpaceCharacters());
			if (valTrim == m_FileReadData->Text)
			{
				SaveFileAsync();
				TextView().ClearHistory();
				return false;
			}
			else
			{
				if (TextView().CanUndo())
				{
					if (valTrim != TextView().PreviousText()) goto invalidateState;
				}
				else goto invalidateState;
			}
		}

		return false;
	invalidateState:
		if (SettingsViewModel::Instance().AutoSave()) SaveFileAsync();
		else Saved(false);
		return true;
	}

	fire_and_forget CodeEditor::LoadFileAsync()
	{
		if (!m_LoadSaveLock)
		{
			m_LoadSaveLock = true;
			bool isRichText = WorkingFile().FileType() == L".rtf" || WorkingFile().FileType() == L".RTF";
			TextView().Initialize(isRichText);
			if (isRichText)
			{
				auto stream = co_await WorkingFile().OpenReadAsync();
				TextView().TextDocument().LoadFromStream(TextSetOptions::FormatRtf, stream);
			}
			else
			{
				auto readBuffer = co_await FileIO::ReadBufferAsync(WorkingFile());
				m_FileReadData = EncodingDetect::DetectPlusGet(readBuffer);
				m_FileReadData->Text = TextEditor::UI::SyntaxEditor::NormalizeLineEndings(Helpers::TrimEnd(m_FileReadData->Text, Helpers::SpaceCharacters()));
				EncodingBox().SelectedIndex(static_cast<int>(m_FileReadData->Encoding));
				TextView().HistoryDone({ this, &CodeEditor::AfterUndoRedoInvoked });
				TextView().Text(m_FileReadData->Text);
			}

			Saved(true);
			TextView().TextChangeDelegate({ this, &CodeEditor::TextChanged });
			Ring().IsActive(false);
			Content().Visibility(Visibility::Visible);
			m_KeyPressHandlerToken = CoreApplication::GetCurrentView().CoreWindow().Dispatcher().AcceleratorKeyActivated({ this, &CodeEditor::KeyPressHandler });
			m_LoadSaveLock = false;
		}

	}
	IAsyncAction CodeEditor::SaveFileAsync()
	{
		if (!m_FileReadData) return;
		if (!m_LoadSaveLock)
		{
			m_LoadSaveLock = true;
			CachedFileManager::DeferUpdates(WorkingFile());
			if (TextView().IsRichText())
			{
				auto stream = co_await WorkingFile().OpenAsync(FileAccessMode::ReadWrite);
				TextView().Document().SaveToStream(TextGetOptions::FormatRtf, stream);
			}
			else co_await FileIO::WriteTextAsync(WorkingFile(), TextView().Text(), static_cast<Streams::UnicodeEncoding>(EncodingBox().SelectedIndex()));
			auto result = co_await CachedFileManager::CompleteUpdatesAsync(WorkingFile());
			if (result != Provider::FileUpdateStatus::Complete) NotificationHelper::Instance().SendBasicNotification(ResourceController::GetTranslation(L"FileSaveFail"), 
				Helpers::IndexStringFormat(ResourceController::GetTranslation(L"FileSaveFailMessage"), { box_value(WorkingFile().Name()) }));
			Saved(true);
			m_LoadSaveLock = false;
		}
	}

	IAsyncAction CodeEditor::CloseAsync() 
	{
		co_await SaveFileAsync();
		Close();
	}

	void CodeEditor::Close()
	{
		TextView().Close();
		CoreApplication::GetCurrentView().CoreWindow().Dispatcher().AcceleratorKeyActivated(m_KeyPressHandlerToken);
		m_FileReadData.reset();
	}

	bool CodeEditor::PrepareClose() 
	{
		Bindings->StopTracking();
		return true;
	}

	void CodeEditor::KeyPressHandler(CoreDispatcher const&, AcceleratorKeyEventArgs const& e)
	{
		if (AppSettings::DialogShown() || m_Unloaded) return;
		if (e.EventType() == CoreAcceleratorKeyEventType::KeyDown 
			&& (CoreApplication::GetCurrentView().CoreWindow().GetKeyState(VirtualKey::Control) & CoreVirtualKeyStates::Down) == CoreVirtualKeyStates::Down)
		{
			if (e.VirtualKey() == VirtualKey::Z)
			{
				e.Handled(true);
				TextView().Undo();
			}
			else if (e.VirtualKey() == VirtualKey::Y)
			{
				e.Handled(true);
				TextView().Redo();
			}
			else if (e.VirtualKey() == VirtualKey::S)
			{
				e.Handled(true);
				SaveFile_Click(nullptr, nullptr);
			}
		}
	}

	void CodeEditor::UserControl_Loaded(IInspectable const&, RoutedEventArgs const&)
	{
		m_Unloaded = false;
		if (Ring().IsActive()) LoadFileAsync();
	}

	void CodeEditor::UserControl_Unloaded(IInspectable const&, RoutedEventArgs const&)
	{
		m_Unloaded = true;
	}

	void CodeEditor::SaveFile_Click(IInspectable const&, RoutedEventArgs const&) { SaveFileAsync(); }
}