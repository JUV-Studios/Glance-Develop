#pragma once
#include "pch.h"
#include <winrt/GraphicsLib.h>

namespace Preferences
{
	extern winrt::Windows::Foundation::Collections::IVectorView<winrt::hstring> SupportedFileTypes();

	extern winrt::Windows::Foundation::IAsyncAction Initialize();
}