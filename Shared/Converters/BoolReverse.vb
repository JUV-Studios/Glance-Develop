Namespace Converters
	<Metadata.WebHostHidden>
	Public NotInheritable Class BoolReverse
		Implements IValueConverter

		Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
			Return Not CBool(value)

		End Function

		Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
			Return Convert(value, targetType, parameter, language)
		End Function
	End Class
End Namespace
