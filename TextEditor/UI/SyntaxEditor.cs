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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TextEditor.Lexer;
using TextEditor.Utils;
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
	public delegate void ContentChangedEventHandler(bool isReset);

	public enum FindArea
	{
		WholeDocument,
		Selection
	}

	public struct FindOptions
	{
		public bool MatchCase;
		public bool UseRegex;
		public FindArea FindLocation;
	}

	public struct SelectionInfo
	{
		public int SelectionStart;
		public int SelectionEnd;
	}

	public sealed class SyntaxEditor : RichEditBox, INotifyPropertyChanged
	{
		private static string EditorStylesString = null;

		public SyntaxEditor()
		{
			TextDocument.DefaultTabStop = 4;
		}

		#region Text view

		private SelectionInfo _TextSelection = new();

		public SelectionInfo TextSelection => _TextSelection;

		public bool IsRichText { get; private set; }

		public bool RemoveBom { get; set; }

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

		public IAsyncAction LoadFileAsync(StorageFile file) => LoadFile(file).AsAsyncAction();

		private async Task LoadFile(StorageFile file)
		{
			// Set default settings, attach event handlers and load the specified content file
			FileLoaded = false;
			IsRichText = file.FileType == ".rtf" || file.FileType == ".RTF";
			AttachEvents();
			if (Resources.MergedDictionaries.Count == 0)
			{
				if (EditorStylesString == null) EditorStylesString = await PathIO.ReadTextAsync("ms-appx:///TextEditor/Themes/Styles.xaml");
				Resources.MergedDictionaries.Add(XamlReader.Load(EditorStylesString) as ResourceDictionary);
			}

			using var readStream = await file.OpenReadAsync();
			if (!IsRichText)
			{
				TextDocument.UndoLimit = 0;
				if (HistoryStack is FileHistoryStack stack)
				{
					stack.ClearHistory();
				}
				else
				{
					HistoryStack = new FileHistoryStack() 
					{
						TargetEditor = new(this)
					};
				}

				// Apply plain text editor style
				var styles = from item in Resources.MergedDictionaries.First()
							 where item.Value is Style
							 select item;

				Style = (from style in styles
					where style.Key.ToString() == "PlainTextEditorStyle"
					select style.Value as Style).First();

				using var inputStream = readStream.GetInputStreamAt(0);
				using var dataReader = new DataReader(inputStream);
				uint bytesLoadedCount = await dataReader.LoadAsync(Convert.ToUInt32(readStream.Size));
				byte[] buffer = new byte[readStream.Size];
				dataReader.ReadBytes(buffer);
				StringEncoding = Utils.TextEncodingDetect.DetectEncoding(buffer);
				Text = CryptographicBuffer.ConvertBinaryToString(StringEncoding, buffer.AsBuffer());
				OriginalText = Text.TrimEnd();
			}
			else
			{
				TextDocument.UndoLimit = int.MaxValue;
				Document.LoadFromStream(TextSetOptions.FormatRtf, readStream);
				if (HistoryStack is not RichEditHistoryWrapper)
				{
					HistoryStack = new RichEditHistoryWrapper(this);
				}
			}

			FileLoaded = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event ContentChangedEventHandler ContentChanged;

		private async void Editor_KeyDown(object sender, KeyRoutedEventArgs e)
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
				default: 
					e.Handled = false;
					break;
			}
		}

		private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
		{
			_TextSelection.SelectionStart = TextDocument.Selection.StartPosition;
			_TextSelection.SelectionEnd = TextDocument.Selection.StartPosition;
			PropertyChanged?.Invoke(this, new(nameof(SelectionText)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectionValid)));
		}

		public bool IsSelectionValid => _TextSelection.SelectionStart != _TextSelection.SelectionEnd && !string.IsNullOrWhiteSpace(SelectionText);

		public string SelectionText
		{
			get => TextDocument.GetRange(_TextSelection.SelectionStart, _TextSelection.SelectionEnd).Text;
			set => TextDocument.GetRange(_TextSelection.SelectionStart, _TextSelection.SelectionEnd).Text = value;
		}

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

		private LineEnding _DesiredLineEnding;

		public LineEnding DesiredLineEnding
		{
			get => _DesiredLineEnding;
			set
			{
				if (_DesiredLineEnding != value)
				{
					_DesiredLineEnding = value;
					PropertyChanged?.Invoke(this, new(nameof(DesiredLineEnding)));
				}
			}
		}

		public string OriginalText { get; private set; }

		public string Text
		{
			get
			{
				TextDocument.GetText(IsRichText ? TextGetOptions.FormatRtf : LineEndingHelper.GetRichRetriveOptions(_DesiredLineEnding), out string text);
				return text;
			}
			set
			{
				if ((HistoryStack.CanUndo || HistoryStack.CanRedo) && !IsRichText)
				{
					var range = TextDocument.GetRange(0, 0);
					range.EndOf(TextRangeUnit.Story, true);
					range.Text = value;
				}
				else
				{
					if (!IsRichText && RemoveBom)
					{
						// Remove BOM
						StringHelper.ReplaceCharacter(value, '\uFEFF', string.Empty[0]);
					}

					TextDocument.SetText(IsRichText ? TextSetOptions.FormatRtf : TextSetOptions.None, value);
				}
			}
		}

		public IAsyncOperation<bool> WriteFileAsync(StorageFile file) => WriteFile(file).AsAsyncOperation();

		private static readonly IBuffer EmptyBuffer = (new byte[] { 0 }).AsBuffer();

		private async Task<bool> WriteFile(StorageFile file)
		{
			CachedFileManager.DeferUpdates(file);
			if (IsRichText)
			{
				using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
				TextDocument.SaveToStream(TextGetOptions.UseCrlf, stream);
			}
			else
			{
				var text = Text;
				await FileIO.WriteBufferAsync(file, string.IsNullOrEmpty(text) ? EmptyBuffer : CryptographicBuffer.ConvertStringToBinary(Text, _StringEncoding));
			}

			var writeResult = await CachedFileManager.CompleteUpdatesAsync(file);
			return writeResult == FileUpdateStatus.Complete;
		}

		private IAsyncAction PrintAsync() => Print().AsAsyncAction();

		private Task Print()
		{
			throw new NotImplementedException();
		}

		private static readonly string[] LineSplitValue = new string[] { Environment.NewLine };

		public int LinesCount => Text.TrimEnd().Split(LineSplitValue, StringSplitOptions.None).Length;

		private bool TextChangedHandler(string text, out bool isReset)
		{
			isReset = false;
			var value = text.TrimEnd();
			bool acceptChanges;
			if (value == OriginalText)
			{
				if (HistoryStack.CanUndo || HistoryStack.CanRedo) HistoryStack.ClearHistory();
				isReset = true;
				acceptChanges = false;
			}
			else if (HistoryStack.TryUndoPeek(out string previousText)) acceptChanges = !(value == previousText);
			else acceptChanges = true;
			return acceptChanges;
		}

		private void Editor_TextChanged(object sender, RoutedEventArgs e)
		{
			var text = Text;
			bool result = TextChangedHandler(text, out bool isReset);
			ContentChanged?.Invoke(isReset);
			if (IsRichText) (HistoryStack as RichEditHistoryWrapper).UpdateHistoryProperties();
			else if (result && HistoryStack is FileHistoryStack stack)
			{
				FileHistoryData val = new(text, TextDocument.Selection.EndPosition);
				stack.UndoPush(ref val);
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

		#region Text suggestions

		public IDictionary<string, TokenType> SuggestionsList { get; } = new Dictionary<string, TokenType>();

		public IEnumerable<KeyValuePair<string, TokenType>> FindSuggestions(string text)
		{
			return from suggestion in SuggestionsList 
				   where suggestion.Key.Contains(text) || text.Contains(suggestion.Key) 
				   select suggestion;
		}

		#endregion

		#region Highlighting

		private static bool HighlightLock = false;

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

		private IHistoryStack _HistoryStack;

		public IHistoryStack HistoryStack
		{
			get => _HistoryStack;
			set
			{
				if (_HistoryStack != value)
				{
					_HistoryStack = value;
					PropertyChanged?.Invoke(this, new(nameof(HistoryStack)));
				}
			}
		}

		public void SelectAll()
		{
			Focus(FocusState.Keyboard);
			TextDocument.Selection.StartPosition = 0;
			TextDocument.Selection.EndOf(TextRangeUnit.Story, true);
		}

		public void ClearSelection() => TextDocument.Selection.EndPosition = TextDocument.Selection.StartPosition;

		public string[] FindText(string text, FindOptions options)
		{
			return null;
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

		#endregion
	}
}