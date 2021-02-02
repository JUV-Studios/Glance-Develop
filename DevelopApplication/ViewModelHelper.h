#pragma once
#include "pch.h"

struct ViewModelHelper
{
	inline void RaisePropertyChanged(winrt::hstring const& propertyName, winrt::Windows::Foundation::IInspectable const& sender)
	{
		PropertyChanged(sender, winrt::Windows::UI::Xaml::Data::PropertyChangedEventArgs(propertyName));
	}

	winrt::event<winrt::Windows::UI::Xaml::Data::PropertyChangedEventHandler> PropertyChanged;
};