Public Interface IAsyncClosable
	Inherits IDisposable

	Function StartClosing() As Boolean

	Function CloseAsync() As IAsyncAction

End Interface
