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
		public WeakReference<SyntaxEditor> TargetEditor { get; init; }

		public bool CanUndo => TargetEditor.ResolveEditorReference().Document.CanUndo();

		public bool CanRedo => TargetEditor.ResolveEditorReference().Document.CanRedo();

		public event PropertyChangedEventHandler PropertyChanged;

		public void UpdateHistoryProperties()
		{
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
		}

		public void ClearHistory() => TargetEditor.ResolveEditorReference().TextDocument.ClearUndoRedoHistory();

		public void Undo() => TargetEditor.ResolveEditorReference().Document.Undo();

		public void Redo() => TargetEditor.ResolveEditorReference().Document.Redo();

		public bool TryRedoPeek(out string text) => throw new NotImplementedException();

		public bool TryUndoPeek(out string text) => throw new NotImplementedException();
	}

	internal sealed class FileHistoryStack : IHistoryStack
	{
		private readonly Stack<(string, int)> UndoStack = new();

		private readonly Stack<(string, int)> RedoStack = new();

		public event PropertyChangedEventHandler PropertyChanged;

		public WeakReference<SyntaxEditor> TargetEditor { get; init; }

		public bool CanUndo => UndoStack.Count > 0;

		public bool CanRedo => RedoStack.Count > 0;

		public void UndoPush(ref (string, int) value)
		{
			UndoStack.Push(value);
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
		}

		public void RedoPush(ref (string, int) value)
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
				text = UndoStack.Peek().Item1;
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
				text = RedoStack.Peek().Item1;
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
			editor.Text = val.Item1;
			editor.Focus(FocusState.Keyboard);
			editor.TextDocument.Selection.StartPosition = val.Item2;
		}

		public void Redo()
		{
			if (!CanRedo) return;
			var editor = TargetEditor.ResolveEditorReference();
			var val = RedoImpl();
			editor.Text = val.Item1;
			editor.Focus(FocusState.Keyboard);
			editor.TextDocument.Selection.StartPosition = val.Item2;
		}

		private (string, int) UndoImpl()
		{
			RedoStack.Push(UndoStack.Pop());
			var result = UndoStack.Pop();
			PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
			return result;
		}

		public (string, int) RedoImpl()
		{
			var result = RedoStack.Pop();
			PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
			return result;
		}
	}
}