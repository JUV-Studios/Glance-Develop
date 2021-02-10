#include "pch.h"
#include "ShellViewModel.h"
#if __has_include("ShellViewModel.g.cpp")
#include "ShellViewModel.g.cpp"
#endif
#include <winrt/DevelopManaged.h>
#include <winrt/JUVStudios.h>
#include "AppSettings.h"

using namespace winrt;
using namespace DevelopManaged;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
	ShellViewModel::ShellViewModel()
	{
		m_Instances = single_threaded_observable_vector<ShellView>();
		SymbolIconSource iconSource;
		iconSource.Symbol(Symbol::Home);
		auto startPageView = ShellView(JUVStudios::ResourceController::GetTranslation(L"HomePage/Header"), HomePage(), iconSource, nullptr);
		AddInstances(array_view<ShellView>(&startPageView, 1));
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	ShellView ShellViewModel::SelectedInstance() { return m_Bindable.GetProperty(L"SelectedInstance").as<ShellView>(); }

	void ShellViewModel::SelectedInstance(ShellView const& value) 
	{
		if (!AppSettings::DialogShown()) m_Bindable.SetProperty(L"SelectedInstance", value);
	}

	void ShellViewModel::AddInstances(array_view<ShellView> instances)
	{
		for (auto&& instance : instances) m_Instances.Append(instance);
		SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
	}

	void ShellViewModel::AddStorageItems(IVectorView<IStorageItem2> const& sItems)
	{
		std::vector<ShellView> viewsToAdd;
		for (auto&& item : sItems)
		{
			ShellView instance = nullptr;
			if (StorageItemOpen(item, &instance))
			{
				if (sItems.Size() == 1) SelectedInstance(instance);
			}
			else if (item.IsOfType(StorageItemTypes::File))
			{
				StorageFile file = item.as<StorageFile>();
				SymbolIconSource iconSource;
				iconSource.Symbol(Symbol::Document);
				viewsToAdd.emplace_back(file.Name(), CodeEditor(file), iconSource, file);
			}
			else
			{

			}
		}

		if (viewsToAdd.size() > 0) AddInstances(viewsToAdd);
	}

	bool ShellViewModel::StorageItemOpen(IStorageItem2 const& item, ShellView* foundItem)
	{
		for (auto&& instance : m_Instances)
		{
			if (instance.ReferenceSource() != nullptr)
			{
				if (instance.ReferenceSource().IsEqual(item))
				{
					*foundItem = instance;
					return true;
				}
			}
		}

		return false;
	}
}
