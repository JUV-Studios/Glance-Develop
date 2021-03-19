// Copyright (c) Adnan Umer. All rights reserved. Follow me @aztnan
// Email: aztnan@outlook.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TextEditor.Lexer;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace TextEditor.UI
{
	internal readonly struct FileHistoryData
	{
		internal FileHistoryData(string text, int selectionIndex)
		{
			Text = text;
			SelectionIndex = selectionIndex;
		}

		public readonly string Text;
		public readonly int SelectionIndex;
	}

	public struct SelectionInfo
	{
		public uint SelectionStart;
		public uint SelectionEnd;
	}

	public sealed class SyntaxEditor : RichEditBox, INotifyPropertyChanged, IDisposable
	{
		private static string EditorStylesString = string.Empty;

		public SyntaxEditor()
		{
			TextDocument.DefaultTabStop = 4;
		}

		#region Text View

		private SelectionInfo _TextSelection = new();

		public SelectionInfo TextSelection => _TextSelection;

		public bool IsRichText { get; private set; }

		private bool _FileLoaded = false;

		public bool FileLoaded
		{
			get => _FileLoaded;
			set
			{
				if (_FileLoaded != value)
				{
					_FileLoaded = value;
					PropertyChanged?.Invoke(this, new(nameof(FileLoaded)));
				}
			}
		}

		public IAsyncAction LoadAsync(StorageFile file) => Load(file).AsAsyncAction();

		private async Task Load(StorageFile file)
		{
			// Set default settings and attach event handlers
			FileLoaded = false;
			IsRichText = file.FileType == ".rtf" || file.FileType == ".RTF";
			AttachEvents();
			if (!IsRichText)
			{
				IsSpellCheckEnabled = false;
				TextWrapping = TextWrapping.NoWrap;
				ClipboardCopyFormat = RichEditClipboardFormat.PlainText;
				DisabledFormattingAccelerators = DisabledFormattingAccelerators.All;
				TextDocument.UndoLimit = 0;
			}

			// Read content file
			using var readStream = await file.OpenReadAsync();
			if (IsRichText) Document.LoadFromStream(TextSetOptions.FormatRtf, readStream);
			else
			{
				using var inputStream = readStream.GetInputStreamAt(0);
				using var dataReader = new DataReader(inputStream);
				uint bytesLoadedCount = await dataReader.LoadAsync((uint)readStream.Size);
				byte[] buffer = new byte[readStream.Size];
				dataReader.ReadBytes(buffer);
				StringEncoding = Utils.TextEncodingDetect.DetectEncoding(buffer);
				Text = CryptographicBuffer.ConvertBinaryToString(StringEncoding, buffer.AsBuffer()).Replace("\uFEFF", string.Empty);
				OriginalText = Text.TrimEnd();
			}

			if (EditorStylesString == string.Empty) EditorStylesString = await PathIO.ReadTextAsync("ms-appx:///TextEditor/Themes/Styles.xaml");
			Resources.MergedDictionaries.Add(XamlReader.Load(EditorStylesString) as ResourceDictionary);
			FileLoaded = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private async void Editor_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (!AppSettings.DialogShown)
			{
				switch (e.Key)
				{
					case VirtualKey.Tab:
						e.Handled = true;
						TextDocument.Selection.TypeText("\t");
						break;
					case VirtualKey.Escape:
						e.Handled = true;
						await FocusManager.TryFocusAsync(FocusManager.FindNextFocusableElement(FocusNavigationDirection.Down), FocusState.Keyboard);
						break;
					default: e.Handled = false; break;
				}
			}
		}

		private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
		{
			_TextSelection.SelectionStart = Convert.ToUInt32(TextDocument.Selection.StartPosition);
			_TextSelection.SelectionEnd = Convert.ToUInt32(TextDocument.Selection.EndPosition);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectionValid)));
		}

		public bool IsSelectionValid => IsSelectionValidImpl(TextSelection.SelectionStart, TextSelection.SelectionEnd);

		private bool IsSelectionValidImpl(uint start, uint end) => start != end && !string.IsNullOrWhiteSpace(TextDocument.GetRange(Convert.ToInt32(start), Convert.ToInt32(end)).Text);

		private BinaryStringEncoding _StringEncoding;

		public BinaryStringEncoding StringEncoding
		{
			get => _StringEncoding;
			set
			{
				if (_StringEncoding != value)
				{
					_StringEncoding = value;
					PropertyChanged?.Invoke(this, new(nameof(StringEncoding)));
				}
			}
		}

		public string OriginalText { get; private set; }

		public string Text
		{
			get
			{
				TextDocument.GetText(TextGetOptions.UseCrlf, out string text);
				return text;
			}
			set
			{
				if (CanUndo || CanRedo)
				{
					var range = TextDocument.GetRange(0, 0);
					range.EndOf(TextRangeUnit.Story, true);
					range.Text = value;
				}
				else TextDocument.SetText(TextSetOptions.None, value);
			}
		}

		public IAsyncOperation<bool> WriteFileAsync(StorageFile file) => WriteFile(file).AsAsyncOperation();

		private async Task<bool> WriteFile(StorageFile file)
		{
			bool result;
			CachedFileManager.DeferUpdates(file);
			if (IsRichText)
			{
				using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
				TextDocument.SaveToStream(TextGetOptions.UseCrlf, stream);
			}
			else
			{
				await FileIO.WriteBufferAsync(file, CryptographicBuffer.ConvertStringToBinary(Text, _StringEncoding));
			}

			var writeResult = await CachedFileManager.CompleteUpdatesAsync(file);
			if (writeResult != FileUpdateStatus.Complete) result = false;
			else result = true;
			return result;
		}

		private static readonly string[] LineSplit = new string[] { Environment.NewLine };

		public int LinesCount => Text.TrimEnd().Split(LineSplit, StringSplitOptions.None).Length;

		private void Editor_TextChanged(object sender, RoutedEventArgs e)
		{
			var text = Text;
			if (TextChangedHandler(text) && !IsRichText)
			{
				UndoStack.Push(new(text, TextDocument.Selection.EndPosition));
				PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
				PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
			}
		}

		private async void Editor_Paste(object sender, TextControlPasteEventArgs e)
		{
			e.Handled = true;
			var content = Clipboard.GetContent();
			if (content.Contains("Text"))
			{
				var text = await content.GetTextAsync();
				TextDocument.Selection.TypeText(text);
			}
		}

		private bool TextChangedHandler(string text)
		{
			bool acceptChanges = false;
			var value = text.TrimEnd();
			if (value == OriginalText)
			{
				if (CanUndo || CanRedo)
				{
					PropertyChanged?.Invoke(this, new("Client_SaveRequested"));
					UndoStack.Clear();
					RedoStack.Clear();
					PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
					PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
				}

				acceptChanges = false;
			}
			else if (CanUndo && value == PreviousText) acceptChanges = false;
			else if (AppSettings.Preferences.AutoSave)
			{
				PropertyChanged?.Invoke(this, new("Client_SaveRequested"));
				acceptChanges = true;
			}
			else
			{
				PropertyChanged?.Invoke(this, new("Client_InvalidateSaveState"));
				acceptChanges = true;
			}

			return acceptChanges;
		}

		public void AttachEvents()
		{
			TextChanged += Editor_TextChanged;
			KeyDown += Editor_KeyDown;
			SelectionChanged += Editor_SelectionChanged;
			if (!IsRichText) Paste += Editor_Paste;
		}

		public void DetachEvents()
		{
			SelectionChanged -= Editor_SelectionChanged;
			TextChanged -= Editor_TextChanged;
			KeyDown -= Editor_KeyDown;
			if (!IsRichText) Paste -= Editor_Paste;
		}

		#endregion

		#region Highlighting

		bool HighlightLock = false;

		public bool SyntaxHighlighting(IReadOnlyList<HighlightToken> tokens)
		{
			if (tokens.Count <= 0 || HighlightLock) return false;
			HighlightLock = true;
			foreach (var token in tokens)
			{
				var range = TextDocument.GetRange(token.StartIndex, Convert.ToInt32(token.StartIndex + token.Length));
				if (range.CharacterFormat.ForegroundColor != token.Colour) range.CharacterFormat.ForegroundColor = token.Colour;
			}

			HighlightLock = false;
			return true;
		}

		#endregion

		#region Interaction

		public bool CanUndo => IsRichText ? Document.CanUndo() : UndoStack.Count > 0;

		public bool CanRedo => IsRichText ? Document.CanRedo() : RedoStack.Count > 0;

		private readonly Stack<FileHistoryData> UndoStack = new();

		private readonly Stack<FileHistoryData> RedoStack = new();

		public string PreviousText => !IsRichText ? UndoStack.Peek().Text : string.Empty;

		public void Undo()
		{
			if (CanUndo)
			{
				if (IsRichText) Document.Undo();
				else
				{
					// Push current to redo stack
					RedoStack.Push(UndoStack.Pop());

					// Perform undo
					var result = UndoStack.Count == 0 ? new(OriginalText, 0) : UndoStack.Pop();
					HistoryItemDone(ref result);
				}
			}
		}

		public void Redo()
		{
			if (CanRedo)
			{
				// Perform redo
				if (IsRichText) Document.Redo();
				else
				{
					var result = RedoStack.Pop();
					HistoryItemDone(ref result);
				}
			}
		}

		private void HistoryItemDone(ref FileHistoryData data)
		{
			Text = data.Text;
			Focus(FocusState.Keyboard);
			TextDocument.Selection.StartPosition = data.SelectionIndex;
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
		}

		public void SelectAll()
		{
			Focus(FocusState.Keyboard);
			TextDocument.Selection.StartPosition = 0;
			TextDocument.Selection.EndOf(TextRangeUnit.Story, true);
		}

		public void ClearSelection() => TextDocument.Selection.EndPosition = TextDocument.Selection.StartPosition;

		public void FindText(string text)
		{

		}

		public void ScrollToLine(int line, bool extend)
		{
			Focus(FocusState.Keyboard);
			TextDocument.Selection.HomeKey(TextRangeUnit.Story, false);
			TextDocument.Selection.MoveStart(TextRangeUnit.Line, line - 1);
			if (extend)
			{
				TextDocument.Selection.Expand(TextRangeUnit.Line);
				TextDocument.Selection.EndPosition = TextDocument.Selection.EndPosition - 1;
			}
		}

		public void Dispose()
		{
			DetachEvents();
		}

		#endregion
	}
}