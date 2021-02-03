#include "pch.h"
#include "ShellViewModel.h"
#if __has_include("ShellViewModel.g.cpp")
#include "ShellViewModel.g.cpp"
#endif
#include "HomePage.h"

using namespace winrt;
using namespace DevelopManaged;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
	ShellViewModel::ShellViewModel()
	{
		m_Instances = single_threaded_observable_vector<ShellView>();
		SymbolIconSource iconSource;
		iconSource.Symbol(Symbol::Home);
		auto startPageView = ShellView(Develop::AppSettings::GetLocalized(L"HomePage/Header"), make<HomePage>(), iconSource, nullptr);
		AddInstances((single_threaded_vector<ShellView>({ startPageView }).GetView()));
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	ShellView ShellViewModel::SelectedInstance() { return m_SelectedItem; }

	void ShellViewModel::SelectedInstance(ShellView const& value)
	{
		if (m_SelectedItem != value)
		{
			m_SelectedItem = value;
			m_PropertyChanged(*this, PropertyChangedEventArgs(L"SelectedInstance"));
		}
	}

	void ShellViewModel::AddInstances(IVectorView<ShellView> views)
	{
		for (auto&& view : views) m_Instances.Append(view);
		SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
	}

	event_token ShellViewModel::PropertyChanged(PropertyChangedEventHandler const& handler) { return m_PropertyChanged.add(handler); }

	void ShellViewModel::PropertyChanged(winrt::event_token const& token) noexcept { m_PropertyChanged.remove(token); }
}
