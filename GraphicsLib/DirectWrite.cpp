#include "pch.h"
#include "DirectWrite.h"

using namespace Platform;
using namespace Platform::Collections;
using namespace GraphicsLib;
using namespace Windows::Foundation::Collections;

DirectWrite::DirectWrite()
{
}

IVectorView<String^>^ DirectWrite::FontsList::get()
{
	if (m_FontsList == nullptr)
	{
		m_FontsList = ref new Vector<String^>();
	}

	return m_FontsList->GetView();
}