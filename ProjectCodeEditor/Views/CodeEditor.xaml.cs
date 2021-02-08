using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class CodeEditor : UserControl
    {
        /* public EditorViewModel ViewModel { get; private set; }

        private readonly Dictionary<VirtualKey, Action> FastCtrlKeyboardShortcuts;

        public CodeEditor(object args)
        {
            if (args is StorageFile file) ViewModel = new(file);
            else ViewModel = args as EditorViewModel;
            InitializeComponent();
            FastCtrlKeyboardShortcuts = new()
            {
                { VirtualKey.Z, ViewModel.Undo },
                { VirtualKey.Y, ViewModel.Redo },
            };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Unloaded)
            {
                Debug.WriteLine("Editor loaded");
                ViewModel.Unloaded = false;
                ViewService.KeyShortcutPressed += ViewService_KeyShortcutPressed;
                ViewService.ShareCharm.DataRequested += ShareCharm_DataRequested;
            }

            if (!ViewModel.PropertiesSet)
            {
                ViewModel.PropertiesSet = true;
                AutomationProperties.SetName(Editor.TextView, ViewModel.FileLocation);
                AutomationProperties.SetHelpText(Editor.TextView, "EditorAutomationEscHelp".GetLocalized());
                Editor.PropertyChanged += Editor_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                await ViewModel.LoadFileAsync();
            }

            ViewService.AppClosingEvent.Add(ViewModel.SaveAsync);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HistoryItemDone")
            {
                Editor.Focus(FocusState.Keyboard);
                if (ViewModel.HistoryRange != -1)
                {
                    Editor.TextView.TextDocument.Selection.StartPosition = ViewModel.HistoryRange;
                    Editor.TextView.TextDocument.Selection.EndPosition = ViewModel.HistoryRange - 1;
                    Editor.TextView.TextDocument.Selection.Collapse(false);
                }
                else Editor.TextView.TextDocument.Selection.HomeKey(TextRangeUnit.Story, false);
            }
            else if (e.PropertyName == "RetriveSelection")
            {
                ViewModel.PropertyChangeReturn = Editor.TextView.TextDocument.Selection.EndPosition;
            }
            else if (e.PropertyName == "IsLoading")
            {
                SplashScreen.Fade(0, 5000);
                SplashScreen.Visibility = Visibility.Collapsed;
            }
        }

        private void Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                // Text changed
                ViewModel.UserContent = Editor.Text;
            }
        }

        private void ViewService_KeyShortcutPressed(object sender, KeyShortcutPressedEventArgs e)
        {
            if (e.Accelerator.IsEnabled && e.Accelerator.Modifiers == VirtualKeyModifiers.Control)
            {
                if (FastCtrlKeyboardShortcuts.TryGetValue(e.Accelerator.Key, out Action function))
                {
                    e.SystemArgs.Handled = true;
                    function();
                }
            }
        }

        private void ShareCharm_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            if (Editor.IsSelectionValid)
            {
                args.Request.Data.Properties.Title = "ShareSelectionTitle".GetLocalized();
                args.Request.Data.Properties.Description = string.Format("ShareSelectionCaption".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.SetText(Editor.TextView.TextDocument.GetRange(Editor.TextSelection.Item1, Editor.TextSelection.Item2).Text);
            }
            else
            {
                ViewModel.Save();
                args.Request.Data.Properties.Title = string.Format("ShareFileTitle".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.Properties.Description = string.Format("ShareFileCaption".GetLocalized(), ViewModel.WorkingFile.Name);
                args.Request.Data.SetStorageItems(new StorageFile[] { ViewModel.WorkingFile });
                args.Request.Data.SetText(Editor.Text);
            }

            deferral.Complete();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Unloaded)
            {
                ViewModel.Unloaded = true;
                Debug.WriteLine("Editor unloaded");
                ViewService.KeyShortcutPressed -= ViewService_KeyShortcutPressed;
                ViewService.ShareCharm.DataRequested -= ShareCharm_DataRequested;
                if (ViewModel.TabClosing) Dispose();
            }
        }

        public void Dispose()
        {
            Editor.PropertyChanged -= Editor_PropertyChanged;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            Editor.Dispose();
            Bindings.StopTracking();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void GoTo_Click(object sender, EventArgs e)
        {
            var lines = ViewModel.UserContent.TrimEndings().Split("\r").Length;
            Editor.DetachEvents(Editor.TextView);
            GoToDialog.LineBox.Text = string.Empty;
            GoToDialog.LineBox.PlaceholderText = $"{"GoToLineBox/Placeholder".GetLocalized()} (1 - {lines})";
            GoToDialog.LineBox.Maximum = lines;
            DialogHelper.ShowPlusBlock(GoToDialog.DialogRef, (result) =>
            {
                if (result == ContentDialogResult.Primary)
                {
                    int lineVal;
                    try
                    {
                        lineVal = Convert.ToInt32(GoToDialog.LineBox.Value);
                    }
                    catch (OverflowException) { lineVal = 0; }
                    Editor.ScrollToLine(lineVal, GoToDialog.ExtendCheckBox.IsChecked.GetValueOrDefault());
                }

                Editor.AttachEvents(Editor.TextView);
            });
        }

        private void StandardUICommand_CanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args) => args.CanExecute = !ViewModel.Unloaded;

        private void PanelPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                (sender as Pivot).IsHeaderItemsCarouselEnabled = false;
            }
        }

        private void GoToShortcut_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            GoTo_Click(this, null);
        }

        public async Task<bool> CloseAsync(bool showDialog = true)
        {
            ViewModel.TabClosing = true;
            if (!ViewModel.Saved)
            {
                if (Preferences.AppSettings.AutoSave) ViewModel.Save();
                else
                {
                    if (!ViewModel.Unloaded && showDialog)
                    {
                        var dialog = Singleton<UnsavedChangesDialog>.Instance;
                        if (!DialogHelper.PreparePresentation())
                        {
                            await dialog.ShowAsync();
                            if (dialog.Result == ContentDialogResult.None) ViewModel.TabClosing = false;
                            else if (dialog.Result == ContentDialogResult.Primary) ViewModel.Save();
                            DialogHelper.EndPresentation();
                        }
                    }
                    else ViewModel.Save();
                }
            }

            if (ViewModel.TabClosing) ViewService.AppClosingEvent.Remove(ViewModel.SaveAsync);
            return ViewModel.TabClosing;
        } */
    }
}