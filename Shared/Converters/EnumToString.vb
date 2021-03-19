Namespace Converters
	<Metadata.WebHostHidden>
	Public NotInheritable Class EnumToString
		Implements IValueConverter

		Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
			Try
				Return [Enum].GetName(value.GetType(), value)
			Catch
				Return String.Empty
			End Try
		End Function

		Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
			Return [Enum].Parse(Type.GetType(parameter), value)
		End Function
	End Class
End Namespace
