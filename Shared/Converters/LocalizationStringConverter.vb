Namespace Converters
	<Metadata.WebHostHidden>
	Public NotInheritable Class LocalizationStringConverter
		Implements IValueConverter

		Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
			Return Resources.ResourceLoader.GetForViewIndependentUse().GetString(value)
		End Function

		Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
			Throw New NotImplementedException()
		End Function
	End Class
End Namespace