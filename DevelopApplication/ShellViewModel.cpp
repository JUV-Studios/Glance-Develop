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
		ShellView startPageView;
		SymbolIconSource iconSource;
		iconSource.Symbol(Symbol::Home);
		startPageView.Title(General::GetLocalized(L"HomePage/Header"));
		startPageView.IconSource(iconSource);
		startPageView.Content(make<HomePage>());
		startPageView.ReferenceSource(nullptr);
		AddInstances((single_threaded_vector<ShellView>({ startPageView }).GetView()));
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	ShellView ShellViewModel::SelectedInstance() { return m_SelectedInstance; }

	void ShellViewModel::SelectedInstance(ShellView const& value)
	{
		if (m_SelectedInstance != value)
		{
			m_SelectedInstance = value;
			m_ViewModel.RaisePropertyChanged(L"SelectedInstance", *this);
		}
	}

	void ShellViewModel::AddInstances(IVectorView<ShellView> views)
	{
		std::vector<ShellView> items;
		items.insert(end(items), begin(m_Instances), end(m_Instances));
		for (auto&& view : views) items.emplace_back(view);
		m_Instances.ReplaceAll(items);
		SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
	}

	event_token ShellViewModel::PropertyChanged(PropertyChangedEventHandler const& handler) { return m_ViewModel.PropertyChanged.add(handler); }

	void ShellViewModel::PropertyChanged(event_token const& token) noexcept { m_ViewModel.PropertyChanged.remove(token); }
}
