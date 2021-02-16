#include "pch.h"
#include "ShellViewModel.h"
#if __has_include("ShellViewModel.g.cpp")
#include "ShellViewModel.g.cpp"
#endif
#include "App.h"

using namespace winrt;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
	SymbolIconSource fileIcon = nullptr;

	ShellViewModel::ShellViewModel()
	{
		m_Instances = single_threaded_observable_vector<ShellView>();
		SymbolIconSource iconSource;
		iconSource.Symbol(Symbol::Home);
		auto startPageView = ShellView(JUVStudios::ResourceController::GetTranslation(L"HomePage/Header"), HomePage(), iconSource, nullptr);
		AddInstances({ startPageView });
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	void ShellViewModel::AddInstances(std::vector<ShellView> const& instances)
	{
		std::vector<ShellView> instanceList;
		instanceList.reserve(m_Instances.Size() + instances.size());
		for (auto&& existing : m_Instances) instanceList.push_back(existing);
		for (auto&& newInstance : instances) instanceList.push_back(newInstance);
		m_Instances.ReplaceAll(instanceList);
		SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
	}

	void ShellViewModel::AddStorageItems(IVectorView<IStorageItem2> const& sItems)
	{
		std::vector<ShellView> viewsToAdd;
		for (auto&& item : sItems)
		{
			ShellView instance = nullptr;
			if (StorageItemOpen(item, instance) && sItems.Size() == 1)
			{
				SelectedInstance(instance);
				return;
			}
			else if (item.IsOfType(StorageItemTypes::File))
			{
				StorageFile file = item.as<StorageFile>();
				if (fileIcon == nullptr)
				{
					fileIcon = SymbolIconSource();
					fileIcon.Symbol(Symbol::Document);
				}

				viewsToAdd.emplace_back(file.Name(), CodeEditor(file), fileIcon, file);
			}
			else
			{

			}
		}

		if (viewsToAdd.size() > 0) AddInstances(viewsToAdd);
	}

	bool ShellViewModel::StorageItemOpen(IStorageItem2 const& item, ShellView& foundItem)
	{
		for (auto&& instance : m_Instances)
		{
			if (instance.ReferenceSource() != nullptr)
			{
				if (instance.ReferenceSource().IsEqual(item))
				{
					foundItem = instance;
					return true;
				}
			}
		}

		return false;
	}
}
