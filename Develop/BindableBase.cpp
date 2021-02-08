#include "pch.h"
#include "BindableBase.h"
#if __has_include("BindableBase.g.cpp")
#include "BindableBase.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::UI::Xaml::Data;

namespace winrt::Develop::implementation
{
	void BindableBase::RaisePropertyChanged(hstring const& propertyName) noexcept
	{
		m_PropertyChanged(*this, PropertyChangedEventArgs(propertyName));
	}

	IInspectable BindableBase::GetProperty(hstring const& propertyName) noexcept
	{
		if (m_Properties.HasKey(propertyName)) return m_Properties.Lookup(propertyName);
		else return nullptr;
	}

	void BindableBase::SetProperty(hstring const& propertyName, IInspectable const& value)
	{
		if (m_Properties.TryLookup(propertyName) != value)
		{
			m_Properties.Insert(propertyName, value);
			RaisePropertyChanged(propertyName);
		}
	}

	event_token BindableBase::PropertyChanged(PropertyChangedEventHandler const& handler) { return m_PropertyChanged.add(handler); }

	void BindableBase::PropertyChanged(event_token const& token) noexcept { m_PropertyChanged.remove(token); }
}
