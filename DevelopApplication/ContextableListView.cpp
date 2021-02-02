#include "pch.h"
#include "ContextableListView.h"
#if __has_include("ContextableListView.g.cpp")
#include "ContextableListView.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Controls;

namespace winrt::Develop::implementation
{
	ContextableListView::ContextableListView()
	{
		ContextRequested({ this, &ContextableListView::ListView_ContextRequested });
	}

	MenuFlyout ContextableListView::ItemContextFlyout() { return m_ItemContextFlyout; }

	void ContextableListView::ItemContextFlyout(MenuFlyout const& value)
	{
		if (m_ItemContextFlyout != value) m_ItemContextFlyout = value;
	}

	ContextableListViewFlyoutRequest ContextableListView::RequestFunc() { return m_RequestFunc; }

	void ContextableListView::RequestFunc(ContextableListViewFlyoutRequest const& value)
	{
		if (m_RequestFunc != value) m_RequestFunc = value;
	}

	void ContextableListView::ListView_ContextRequested(UIElement const& sender, ContextRequestedEventArgs const& args)
	{
		args.Handled(true);
		ListViewItem target = nullptr;
		IInspectable param = nullptr;
		if (args.OriginalSource().try_as(target)) param = target.Content(); // Handle keyboard input
		else param = args.OriginalSource().as<FrameworkElement>().DataContext(); // Handle touch & mouse input
		if (m_RequestFunc())
		{
			Point location;
			if (args.TryGetPosition(sender, location)) m_ItemContextFlyout.ShowAt(sender, location);
			else m_ItemContextFlyout.ShowAt(target);
		}
	}
}
