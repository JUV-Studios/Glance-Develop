//
// FindDialog.xaml.cpp
// Implementation of the FindDialog class
//

#include "pch.h"
#include "FindDialog.xaml.h"

using namespace DevelopComponent;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

FindDialog^ instance = nullptr;

FindDialog::FindDialog()
{
	InitializeComponent();
}

FindDialog^ FindDialog::Instance::get()
{
	if (instance == nullptr) instance = ref new FindDialog();

	return instance;
}

void FindDialog::Cancel_Click(ContentDialog^ sender, ContentDialogButtonClickEventArgs^ args) { Hide(); }

void FindDialog::Find_Click(ContentDialog^ sender, ContentDialogButtonClickEventArgs^ args)
{
	m_TextToFind = textFindBox->Text;
	Hide();
}