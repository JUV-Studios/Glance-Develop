#include "Interactions.h"
#if __has_include("Interactions.g.cpp")
#include "Interactions.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Foundation;

namespace winrt::Develop::implementation
{
	IAsyncAction Interactions::OpenFile()
	{
		return Windows::Foundation::IAsyncAction();
	}
}