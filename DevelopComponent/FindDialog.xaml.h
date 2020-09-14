//
// FindDialog.xaml.h
// Declaration of the FindDialog class
//

#pragma once

#include "FindDialog.g.h"

namespace DevelopComponent
{
	[Windows::Foundation::Metadata::WebHostHidden]
	public ref class FindDialog sealed
	{
	public:
		FindDialog();

		static property FindDialog^ Instance
		{
			FindDialog^ get();
		}

		property Platform::String^ TextToFind
		{
			Platform::String^ get() { return m_TextToFind; };
		}
	private:
		Platform::String^ m_TextToFind;
		void Cancel_Click(Windows::UI::Xaml::Controls::ContentDialog^ sender, Windows::UI::Xaml::Controls::ContentDialogButtonClickEventArgs^ args);
		void Find_Click(Windows::UI::Xaml::Controls::ContentDialog^ sender, Windows::UI::Xaml::Controls::ContentDialogButtonClickEventArgs^ args);
	};
}
