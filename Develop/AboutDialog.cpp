#include "AboutDialog.h"
#if __has_include("AboutDialog.g.cpp")
#include "AboutDialog.g.cpp"
#endif
#include "App.h"

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;

namespace winrt::Develop::implementation
{
	Develop::AboutDialog instance{ nullptr };

	AboutDialog::AboutDialog()
	{
		InitializeComponent();
	}
	
	hstring AboutDialog::OkayTextId() { return L"OkayText"; }

	IAsyncAction AboutDialog::ShowDialogAsync()
	{
		if (!AppSettings::DialogShown())
		{
			auto mainPage = Window::Current().Content().as<MainPage>();
			AppSettings::DialogShown(true);
			if (instance == nullptr) instance = make<AboutDialog>();
			co_await instance.ShowAsync();
			AppSettings::DialogShown(false);
		}
	}

	void AboutDialog::BuiltonBlock_Loaded(IInspectable const& sender, RoutedEventArgs const&)
	{
		auto target = sender.as<TextBlock>();
		if (target.Text().empty()) target.Text(JUVStudios::ResourceController::GetTranslation(L"BuiltOnBlock/Text") + L" " + TEXT(__TIMESTAMP__));
	}
}