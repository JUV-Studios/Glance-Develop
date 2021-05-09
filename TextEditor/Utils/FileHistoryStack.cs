using System;
using System.Collections.Generic;
using System.ComponentModel;
using TextEditor.UI;
using Windows.UI.Xaml;

namespace TextEditor.Utils
{
	public interface IHistoryStack : INotifyPropertyChanged
	{
		public bool CanUndo { get; }

		public bool CanRedo { get; }

		public void Undo();

		public void Redo();

		public void ClearHistory();

		public bool TryUndoPeek(out string text);

		public bool TryRedoPeek(out string text);
	}

	internal static class HistoryStackHelper
	{
		public static SyntaxEditor ResolveEditorReference(this WeakReference<SyntaxEditor> target)
		{
			if (target.TryGetTarget(out SyntaxEditor editor)) return editor;
			else throw new ObjectDisposedException("SyntaxEditor", "The SyntaxEditor object for this history stack has been disposed");
		}
	}

	internal sealed class RichEditHistoryWrapper : IHistoryStack
	{
		public RichEditHistoryWrapper(SyntaxEditor editor)
		{
			Target = new(editor);
		}

		private readonly WeakReference<SyntaxEditor> Target;

		public bool CanUndo => Target.ResolveEditorReference().Document.CanUndo();

		public bool CanRedo => Target.ResolveEditorReference().Document.CanRedo();

		public event PropertyChangedEventHandler PropertyChanged;

		public void UpdateHistoryProperties()
		{
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
		}

		public void ClearHistory() => Target.ResolveEditorReference().TextDocument.ClearUndoRedoHistory();

		public void Undo() => Target.ResolveEditorReference().Document.Undo();

		public void Redo() => Target.ResolveEditorReference().Document.Redo();

		public bool TryRedoPeek(out string text) => throw new NotImplementedException();

		public bool TryUndoPeek(out string text) => throw new NotImplementedException();
	}

	internal readonly struct FileHistoryData
	{
		public FileHistoryData(string text, int selectionIndex)
		{
			Text = text;
			SelectionIndex = selectionIndex;
		}

		public readonly int SelectionIndex;

		public readonly string Text;
	}

	internal sealed class FileHistoryStack : IHistoryStack
	{
		private readonly Stack<FileHistoryData> UndoStack = new();

		private readonly Stack<FileHistoryData> RedoStack = new();

		public event PropertyChangedEventHandler PropertyChanged;

		public WeakReference<SyntaxEditor> TargetEditor { get; init; }

		public bool CanUndo => UndoStack.Count > 0;

		public bool CanRedo => RedoStack.Count > 0;

		public void UndoPush(ref FileHistoryData value)
		{
			UndoStack.Push(value);
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
		}

		public void RedoPush(ref FileHistoryData value)
		{
			RedoStack.Push(value);
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
		}

		public void ClearHistory()
		{
			UndoStack.Clear();
			RedoStack.Clear();
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
		}

		public bool TryUndoPeek(out string text)
		{
			if (CanUndo)
			{
				text = UndoStack.Peek().Text;
				return true;
			}
			else
			{
				text = string.Empty;
				return false;
			}
		}

		public bool TryRedoPeek(out string text)
		{
			if (CanRedo)
			{
				text = RedoStack.Peek().Text;
				return true;
			}
			else
			{
				text = string.Empty;
				return false;
			}
		}

		public void Undo()
		{
			if (!CanUndo) return;
			var editor = TargetEditor.ResolveEditorReference();
			var val = UndoImpl();
			editor.Text = val.Text;
			editor.Focus(FocusState.Keyboard);
			editor.TextDocument.Selection.StartPosition = val.SelectionIndex;
		}

		public void Redo()
		{
			if (!CanRedo) return;
			var editor = TargetEditor.ResolveEditorReference();
			var val = RedoImpl();
			editor.Text = val.Text;
			editor.Focus(FocusState.Keyboard);
			editor.TextDocument.Selection.StartPosition = val.SelectionIndex;
		}

		private FileHistoryData UndoImpl()
		{
			RedoStack.Push(UndoStack.Pop());
			var result = UndoStack.Pop();
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
			return result;
		}

		public FileHistoryData RedoImpl()
		{
			var result = RedoStack.Pop();
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
			return result;
		}
	}
}