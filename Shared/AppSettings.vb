Imports Windows.Storage
Imports Windows.UI.Notifications
Imports Windows.Data.Xml.Dom

<Metadata.WebHostHidden>
Public NotInheritable Class AppPreferences
	Implements INotifyPropertyChanged

	Private ReadOnly _LocalSettings As ApplicationDataContainer

	Friend Sub New()
		_LocalSettings = ApplicationData.Current.LocalSettings
	End Sub

	Public ReadOnly Property UserInterfaceSettings As UISettings = New UISettings()

	Private Function GetSetting(Of T)(key As String, fallback As T) As T
		If _LocalSettings.Values.ContainsKey(key) Then
			Return _LocalSettings.Values(key)
		Else
			_LocalSettings.Values(key) = fallback
			Return fallback
		End If
	End Function

	Property AutoSave As Boolean
		Get
			Return GetSetting(NameOf(AutoSave), False)
		End Get
		Set(value As Boolean)
			If AutoSave <> value Then
				_LocalSettings.Values(NameOf(AutoSave)) = value
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(AutoSave)))
			End If
		End Set
	End Property

	Property DisableSound As Boolean
		Get
			Return GetSetting(NameOf(DisableSound), False)
		End Get
		Set(value As Boolean)
			If DisableSound <> value Then
				_LocalSettings.Values(NameOf(DisableSound)) = value
				ElementSoundPlayer.State = If(value, ElementSoundPlayerState.Off, ElementSoundPlayerState.On)
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(DisableSound)))
			End If
		End Set
	End Property

	Property FontSize As Double
		Get
			Return GetSetting(NameOf(FontSize), 14.0)
		End Get
		Set(value As Double)
			If FontSize <> value Then
				_LocalSettings.Values(NameOf(FontSize)) = value
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(FontSize)))
			End If
		End Set
	End Property

	Property AppThemeIndex As Integer
		Get
			Return GetSetting(NameOf(AppThemeIndex), 2)
		End Get
		Set(value As Integer)
			If AppThemeIndex <> value Then
				_LocalSettings.Values(NameOf(AppThemeIndex)) = value
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(AppThemeIndex)))
			End If
		End Set
	End Property

	Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class

Public Module AppSettings

	Property DialogShown As Boolean = False

	Public ReadOnly Property Preferences As AppPreferences = New AppPreferences()

	Public Sub ForceGarbageCollection()
		GC.Collect()
		GC.WaitForPendingFinalizers()
	End Sub
End Module
