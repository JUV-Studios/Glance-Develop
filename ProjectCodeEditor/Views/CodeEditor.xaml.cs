using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.UI.Xaml.Controls;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Dialogs;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TextEditor;
using TextEditorUWP.Languages;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class CodeEditor : UserControl, IDisposable
    {
        public EditorViewModel ViewModel { get; private set; }

        private readonly DataTransferManager ShareCharm = DataTransferManager.GetForCurrentView();

        public CodeEditor(Dictionary<string, object> args)
        {
            if (args.TryGetValue("file", out object file))
            {
                var storageFile = file as StorageFile;
                ViewModel = new()
                {
                    WorkingFile = storageFile,
                };

                Singleton<RecentsViewModel>.Instance.AddRecentFile(storageFile);
            }
            else if (args.ContainsKey("viewModel")) ViewModel = args["viewModel"] as EditorViewModel;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Unloaded)
            {
                Debug.WriteLine("Editor loaded");
                ViewModel.Unloaded = false;
                ViewService.Properties.ViewTitle = ViewModel.WorkingFile.Name;
                ViewService.KeyShortcutPressed += ViewService_KeyShortcutPressed;
                ViewService.AppClosingEvent.Add(ViewService_AppClosing);
                ViewModel.CanInteract = true;
                ShareCharm.DataRequested += ShareCharm_DataRequested;
                if (!ViewModel.FileReadData.HasValue) LoadFile();
            }
        }

        private void ViewService_AppClosing()
        {
            ViewModel.Save();
            Close();
        }

        private void ViewService_KeyShortcutPressed(object sender, KeyShortcutPressedEventArgs e)
        {
            // Intercept Ctrl+Z via CoreWindow
            if (e.Accelerator.Modifiers == VirtualKeyModifiers.Control && e.Accelerator.Key == VirtualKey.Z && e.Accelerator.IsEnabled)
            {
                e.SystemArgs.Handled = true;
                Undo_Click(null, null);
            }
            // Intercept Ctrl+Y via CoreWindow
            if (e.Accelerator.Modifiers == VirtualKeyModifiers.Control && e.Accelerator.Key == VirtualKey.Y && e.Accelerator.IsEnabled)
            {
                e.SystemArgs.Handled = true;
                Redo_Click(null, null);
            }
            // Intercept Ctrl+S via CoreWindow
            else if (e.Accelerator.Modifiers == VirtualKeyModifiers.Control && e.Accelerator.Key == VirtualKey.S && e.Accelerator.IsEnabled)
            {
                e.SystemArgs.Handled = true;
                ViewModel.Save();
            }
        }

        private async void LoadFile()
        {
            try
            {
                ViewModel.FileReadData = await FileService.ReadTextFileAsync(ViewModel.WorkingFile);
                string format = ViewModel.WorkingFile.FileType.ToLower();
                if (LanguageProvider.CodeLanguages.TryGetValue(format, out Lazy<SyntaxLanguage> syntaxLang)) ViewModel.CodeLanguage = syntaxLang.Value;
                AutomationProperties.SetHelpText(Editor.TextView, "EditorAutomationEscHelp".GetLocalized());
                AutomationProperties.SetName(Editor.TextView, ViewModel.WorkingFile.Name);
                Editor.Text = ViewModel.FileReadData.Value.Text;
                ViewModel.IsLoading = false;
                Editor.TextChanged += Editor_TextChanged;
            }
            catch (FileNotFoundException)
            {
                Close();
                App.AppSettings.DialogShown = true;
                ContentDialog dialog = new()
                {
                    Title = string.Format("NoFileOpenDialogTitle".GetLocalized(), ViewModel.WorkingFile.Name),
                    Content = string.Format("NoFileOpenDialogContent".GetLocalized(), ViewModel.WorkingFile.Name),
                    DefaultButton = ContentDialogButton.Close,
                    CloseButtonText = "OkayText".GetLocalized()
                };

                await dialog.ShowAsync();
                App.AppSettings.DialogShown = false;
            }
        }

        private string ViewModel_EditorTextRequested() => Editor.Text;

        private void ShareCharm_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            ViewModel.Save();
            if (ViewModel.ShareOption == WhatToShare.File)
            {
                args.Request.Data.Properties.Title = string.Format("ShareFileTitle".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.Properties.Description = string.Format("ShareFileCaption".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.SetStorageItems(new StorageFile[] { ViewModel.WorkingFile });
            }
            else if (ViewModel.ShareOption == WhatToShare.Text)
            {
                args.Request.Data.Properties.Title = string.Format("ShareTextTitle".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.Properties.Description = string.Format("ShareTextCaption".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.SetText(Editor.Text);
            }
            else
            {
                args.Request.Data.Properties.Title = "ShareSelectionTitle".GetLocalized();
                args.Request.Data.Properties.Description = string.Format("ShareSelectionCaption".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.SetText(Editor.TextView.TextDocument.GetRange(Editor.TextSelection.Item1, Editor.TextSelection.Item2).Text);
            }

            deferral.Complete();
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            string text1, text2, text3 = string.Empty;
            if (ViewModel.FileReadData.HasValue)
            {
                text1 = Editor.Text.TrimEnd();
                text2 = ViewModel.FileReadData.Value.Text.TrimEnd();
                if (!ViewModel.UndoStack.IsEmpty()) text3 = ViewModel.UndoStack.Peek().TrimEnd();

                if (text1 == text2)
                {
                    ViewModel.Saved = true;
                    ViewModel.ClearHistory();
                }
                else if (text1 != text2 && text1 != text3)
                {
                    ViewModel.PushToUndoStack(Editor.Text);
                    ViewModel.Saved = false;
                }
            }
            
            if (App.AppSettings.AutoSave) ViewModel.Save();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Unloaded)
            {
                ViewModel.Unloaded = true;
                Debug.WriteLine("Editor unloaded");
                ViewService.Properties.ViewTitle = string.Empty;
                ViewService.KeyShortcutPressed -= ViewService_KeyShortcutPressed;
                ViewModel.CanInteract = false;
                ShareCharm.DataRequested -= ShareCharm_DataRequested;
            }
        }

        public async void Dispose()
        {
            ViewModel.TabClosing = true;
            if (ViewModel.Saved) Close();
            else if (App.AppSettings.AutoSave)
            {
                ViewModel.Save();
                Close();
            }
            else
            {
                if (!App.AppSettings.DialogShown)
                {
                    App.AppSettings.DialogShown = true;
                    var dialog = Singleton<UnsavedChangesDialog>.Instance;
                    await dialog.ShowAsync();
                    App.AppSettings.DialogShown = false;
                    if (dialog.Result == ContentDialogResult.Primary) ViewModel.Save();
                    else if (dialog.Result == ContentDialogResult.Secondary) Close();
                    else
                    {
                        ViewModel.TabClosing = false;
                        return;
                    }
                }
                else ViewModel.Save();
                Close();
            }
        }

        private void Close()
        {
            Editor.TextChanged -= Editor_TextChanged;
            Editor.Dispose();
            if (!ViewService.Properties.AppClosing)
            {
                ViewService.AppClosingEvent.Remove(ViewService_AppClosing);
                Singleton<ShellViewModel>.Instance.RemoveInstance(Singleton<ShellViewModel>.Instance.SelectedItem);
            }
        }

        private async void GoTo_Click(object sender, RoutedEventArgs e)
        {
            App.AppSettings.DialogShown = true;
            Editor.DetachEvents(Editor.TextView);
            var documentText = Editor.Text;
            var lines = documentText.Split("\r");
            var lineBox = new NumberBox()
            {
                Minimum = 1,
                Maximum = lines.Count() - 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                AcceptsExpression = true,
                Header = "GoToLineBox/Header".GetLocalized(),
                PlaceholderText = $"{"GoToLineBox/Placeholder".GetLocalized()} (1 - {lines.Count() - 1})",
            };

            var goToDialog = new ContentDialog()
            {
                Title = "GoToLineItem/Text".GetLocalized(),
                PrimaryButtonText = "GoToDialog/PrimaryButtonText".GetLocalized(),
                SecondaryButtonText = "CancelText".GetLocalized(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                DefaultButton = ContentDialogButton.Primary,
                Content = lineBox
            };

            if (await goToDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                int lineVal;
                try
                {
                    lineVal = Convert.ToInt32(lineBox.Value);
                }
                catch (OverflowException) { lineVal = 0; }
                Editor.ScrollToLine(lineVal);
            }

            Editor.AttachEvents(Editor.TextView);
            App.AppSettings.DialogShown = false;
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Editor_Escaped(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (!ViewModel.IsLoading)
            {
                args.Handled = true;
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        private async void Find_Click(object sender, RoutedEventArgs e)
        {
            if (!App.AppSettings.DialogShown)
            {
                App.AppSettings.DialogShown = true;
                var findDialog = Singleton<FindDialog>.Instance;
                await findDialog.ShowAsync();
                if (findDialog.Result == ContentDialogResult.Primary) Editor.FindText(findDialog.FindText);
                App.AppSettings.DialogShown = false;
            }

        }

        private void StandardUICommand_CanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args) => args.CanExecute = ViewModel.CanInteract;

        private void Undo_Click(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (ViewModel.CanUndo)
            {
                Editor.Text = ViewModel.Undo().TrimEnd();
                Editor.TextView.TextDocument.Selection.EndKey(TextRangeUnit.Story, false);
            }
        }

        private void Redo_Click(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (ViewModel.CanRedo)
            {
                Editor.Text = ViewModel.Redo().TrimEnd();
                Editor.TextView.TextDocument.Selection.EndKey(TextRangeUnit.Story, false);
            }
        }
    }
}
