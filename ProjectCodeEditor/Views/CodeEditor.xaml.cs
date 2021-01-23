﻿using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Dialogs;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls.Extensions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class CodeEditor : UserControl, IDisposable
    {
        public EditorViewModel ViewModel { get; private set; }

        private static readonly DataTransferManager ShareCharm = DataTransferManager.GetForCurrentView();

        private bool PropertiesSet = false;

        internal static readonly string PathOpenCommandId = "FileOpenPathItem/Text";

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
                { VirtualKey.A, Editor.SelectAll },
            };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Unloaded)
            {
                Debug.WriteLine("Editor loaded");
                ViewModel.Unloaded = false;
                ViewService.Properties.ViewTitle = ViewModel.WorkingFile.Name;
                ViewService.KeyShortcutPressed += ViewService_KeyShortcutPressed;
                ShareCharm.DataRequested += ShareCharm_DataRequested;
                await ViewModel.ResumeAsync();
            }

            if (!PropertiesSet)
            {
                PropertiesSet = true;
                AutomationProperties.SetHelpText(Editor.TextView, "EditorAutomationEscHelp".GetLocalized());
                AutomationProperties.SetName(Editor.TextView, ViewModel.WorkingFile.Name);
                Editor.PropertyChanged += Editor_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                try
                {
                    await ViewModel.LoadFileAsync();
                }
                catch (FileNotFoundException)
                {
                    Close();
                    DialogHelper.ShowPlusBlock(new()
                    {
                        Title = string.Format("NoFileOpenDialogTitle".GetLocalized(), ViewModel.WorkingFile.Name),
                        Content = string.Format("NoFileOpenDialogContent".GetLocalized(), ViewModel.WorkingFile.Name),
                        DefaultButton = ContentDialogButton.Close,
                        CloseButtonText = "OkayText".GetLocalized()
                    }, null);
                }

                ViewService.AppClosingEvent.Add(ViewService_AppClosing);
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HistoryItemDone")
            {
                if (ViewModel.HistoryRange != -1)
                {
                    Editor.Focus(FocusState.Keyboard);
                    Editor.TextView.TextDocument.Selection.StartPosition = ViewModel.HistoryRange;
                    Editor.TextView.TextDocument.Selection.EndPosition = ViewModel.HistoryRange - 1;
                    Editor.TextView.TextDocument.Selection.Collapse(false);
                }
            }
            else if (e.PropertyName == "RetriveSelection")
            {
                ViewModel.PropertyChangeReturn = Editor.TextView.TextDocument.Selection.EndPosition;
            }
            else if (e.PropertyName == "IsLoading") FadeSplashScreenAsync().ConfigureAwait(false);
        }

        private async Task FadeSplashScreenAsync()
        {
            await SplashScreen.FadeOutAsync(TimeSpan.FromSeconds(10));
            SplashScreen.Visibility = Visibility.Collapsed;
        }

        private void Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                // Text changed
                ViewModel.UserContent = Editor.Text;
            }
        }

        private void ViewService_AppClosing()
        {
            ViewModel.Save();
            Close();
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Unloaded)
            {
                ViewModel.Unloaded = true;
                ViewModel.SuspendAsync().ConfigureAwait(false);
                Debug.WriteLine("Editor unloaded");
                ViewService.Properties.ViewTitle = string.Empty;
                ViewService.KeyShortcutPressed -= ViewService_KeyShortcutPressed;
                ShareCharm.DataRequested -= ShareCharm_DataRequested;
            }
        }

        public void Dispose()
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
                if (!App.AppSettings.DialogShown && !ViewModel.Unloaded)
                {
                    var dialog = Singleton<UnsavedChangesDialog>.Instance;
                    DialogHelper.ShowPlusBlock(dialog, (e) =>
                    {
                        if (dialog.Result == ContentDialogResult.None)
                        {
                            ViewModel.TabClosing = false;
                            return;
                        }

                        if (dialog.Result == ContentDialogResult.Primary) ViewModel.Save();
                        Close();
                    });
                }
                else
                {
                    ViewModel.Save();
                    Close();
                }
            }
        }

        private void Close()
        {
            Editor.PropertyChanged -= Editor_PropertyChanged;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            Editor.Dispose();
            Bindings.StopTracking();
            if (!ViewService.Properties.AppClosing)
            {
                ViewService.AppClosingEvent.Remove(ViewService_AppClosing);
                ShellView instance = null;
                Interactions.StorageItemAlreadyOpen(ViewModel.WorkingFile, ref instance);
                Singleton<ShellViewModel>.Instance.RemoveInstance(instance);
            }
        }

        private void GoTo_Click(object sender, EventArgs e)
        {
            var lines = ViewModel.UserContent.Split("\r");
            Editor.DetachEvents(Editor.TextView);
            GoToDialog.LineBox.Text = string.Empty;
            GoToDialog.LineBox.PlaceholderText = $"{"GoToLineBox/Placeholder".GetLocalized()} (1 - {lines.Count() - 1})";
            GoToDialog.LineBox.Maximum = lines.Count() - 1;
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
                    Editor.ScrollToLine(lineVal, false);
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

        private void FindBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Editor.FindText(args.QueryText);
        }
    }
}
