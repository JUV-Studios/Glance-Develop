#include "ShellViewModel.h"
#if __has_include("ShellViewModel.g.cpp")
#include "ShellViewModel.g.cpp"
#endif

using namespace winrt;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::Develop::implementation
{
	FontIconSource fileIcon = nullptr;

	ShellViewModel::ShellViewModel()
	{
		m_Instances = single_threaded_observable_vector<ShellView>();
		FontIconSource f;
		f.Glyph(L"\uE10F");
		f.FontFamily(FontFamily(L"Segoe MDL2 Assets"));
		auto startPageView = ShellView(JUVStudios::ResourceController::GetTranslation(L"HomePage/Header"), HomePage(), f, nullptr);
		AddInstances({ startPageView });
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	void ShellViewModel::AddInstances(std::vector<ShellView> const& instances)
	{
		for (auto&& instance : instances) m_Instances.Append(instance);
		SelectedIndex(m_Instances.Size() - 1);
	}

	void ShellViewModel::AddStorageItems(IVectorView<IStorageItem2> const& sItems)
	{
		std::vector<ShellView> viewsToAdd;
		for (auto&& item : sItems)
		{
			ShellView instance = nullptr;
			if (StorageItemOpen(item, &instance))
			{
				if (sItems.Size() == 1 && !AppSettings::DialogShown()) SelectedInstance(instance);
			}
			else if (item.IsOfType(StorageItemTypes::File))
			{
				StorageFile file = item.as<StorageFile>();
				if (AppSettings::IsFileTypeSupported(file.FileType()))
				{
					if (fileIcon == nullptr)
					{
						fileIcon = FontIconSource();
						fileIcon.Glyph(L"\uE130");
						fileIcon.FontFamily(FontFamily(L"Segoe MDL2 Assets"));
					}

					viewsToAdd.emplace_back(file.Name(), CodeEditor(file), fileIcon, file);
				}
				else Launcher::LaunchFileAsync(file);
			}
			else Launcher::LaunchFolderAsync(item.as<StorageFolder>());
		}

		if (viewsToAdd.size() > 0) AddInstances(viewsToAdd);
	}

	IAsyncAction ShellViewModel::RemoveInstance(Develop::ShellView const view)
	{
		uint32_t index = 0;
		auto selectedIndex = SelectedIndex();
		m_Instances.IndexOf(view, index);
		m_Instances.RemoveAt(index);
		if (selectedIndex == index) SelectedIndex(0);
		IAsyncClosable closable;
		if (view.Content().try_as(closable)) co_await closable.CloseAsync();
	}

	uint32_t ShellViewModel::SelectedIndex()
	{
		uint32_t index = 0;
		m_Instances.IndexOf(SelectedInstance(), index);
		return index;
	}

	void ShellViewModel::SelectedIndex(uint32_t index)
	{
		if (!AppSettings::DialogShown()) SelectedInstance(m_Instances.GetAt(index));
	}

	bool ShellViewModel::StorageItemOpen(IStorageItem2 const& item, ShellView* const foundItem)
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
