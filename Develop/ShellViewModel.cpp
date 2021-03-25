#include "ShellViewModel.h"
#if __has_include("ShellViewModel.g.cpp")
#include "ShellViewModel.g.cpp"
#endif
#include <winrt/Shared.h>

using namespace winrt;
using namespace Shared;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::ApplicationModel::Resources;

namespace winrt::Develop::implementation
{
	inline FontIconSource FileIcon()
	{
		static FontIconSource fileIcon = nullptr;
		if (fileIcon == nullptr)
		{
			fileIcon = {};
			fileIcon.Glyph(L"\uE130");
			fileIcon.FontFamily(FontFamily(L"Segoe MDL2 Assets"));
		}

		return fileIcon;
	}

	ShellViewModel::ShellViewModel()
	{
		FontIconSource homeIcon;
		homeIcon.Glyph(L"\uE10F");
		homeIcon.FontFamily(FontFamily(L"Segoe MDL2 Assets"));
		m_StartTab = ShellView(ResourceLoader::GetForViewIndependentUse().GetString(L"StartPageTitle"), StartPage(), homeIcon, nullptr);
		AddInstances({ m_StartTab });
	}

	Develop::ShellViewModel ShellViewModel::Instance()
	{
		static Develop::ShellViewModel instance = nullptr;
		if (instance == nullptr) instance = make<ShellViewModel>();
		return instance;
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	void ShellViewModel::AddInstances(array_view<ShellView const> instances)
	{
		for (auto&& tabInstance : instances) m_Instances.Append(tabInstance);
		SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
	}

	std::map<IStorageItem2, Develop::ShellView> ShellViewModel::MapOpenDocuments(IVectorView<IStorageItem2> const& documents)
	{
		std::map<IStorageItem2, Develop::ShellView> results;
		for (auto&& document : documents)
		{
			results.emplace(document, FindInstance(document));
		}

		return results;
	}

	ShellView ShellViewModel::FindInstance(IStorageItem2 const& refSource)
	{
		for (auto&& tabInstance : m_Instances)
		{
			if (tabInstance == m_StartTab) continue;
			else if (tabInstance.ReferenceSource().IsEqual(refSource)) return tabInstance;
		}

		return nullptr;
	}

	IAsyncAction ShellViewModel::AddStorageItems(IVectorView<IStorageItem2> sItems)
	{
		std::vector<ShellView> viewsToAdd;
		auto existing = MapOpenDocuments(sItems);
		for (auto&& document : existing)
		{
			if (document.second == nullptr)
			{
				if (document.first.IsOfType(StorageItemTypes::File))
				{
					auto file = document.first.as<StorageFile>();
					viewsToAdd.emplace_back(file.Name(), CodeEditor(file), FileIcon(), file);
				}
				else co_await Launcher::LaunchFolderAsync(document.first.as<StorageFolder>());
			}
			else if (sItems.Size() == 1)
			{
				SelectedInstance(document.second);
				co_return;
			}
		}

		if (viewsToAdd.size() > 0) AddInstances(viewsToAdd);
		else SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
	}

	IAsyncAction ShellViewModel::RemoveInstance(ShellView const& view)
	{
		bool isSelected = view == m_SelectedInstance;
		uint32_t index = 0;
		m_Instances.IndexOf(view, index);
		m_Instances.RemoveAt(index);		
		if (isSelected) SelectedInstance(m_StartTab);
		if (auto closable = view.Content().try_as<IAsyncClosable>()) co_await closable.CloseAsync();
		view.Close();
	}

	bool ShellViewModel::TryCloseInstance(ShellView const& view)
	{
		bool removeInstance = false;
		IAsyncClosable closable;
		if (view.Content() == nullptr) removeInstance = true;
		else if (view.Content().try_as(closable)) removeInstance = closable.StartClosing();
		if (removeInstance) RemoveInstance(view);
		return removeInstance;
	}

	void ShellViewModel::FastSwitch(bool reverse)
	{
		uint32_t index;
		m_Instances.IndexOf(m_SelectedInstance, index);
		if (m_Instances.Size() != 1)
		{
			if (reverse)
			{
				if (index == 0) SelectedInstance(m_Instances.GetAt(m_Instances.Size() - 1));
				else SelectedInstance(m_Instances.GetAt(index - 1));
			}
			else
			{
				if (index == m_Instances.Size() - 1) SelectedInstance(m_Instances.GetAt(0));
				else SelectedInstance(m_Instances.GetAt(index + 1));
			}
		}
	}

	ShellView ShellViewModel::SelectedInstance()
	{
		return m_SelectedInstance;
	}

	void ShellViewModel::SelectedInstance(ShellView const& value)
	{
		if (m_SelectedInstance != value && !AppSettings::DialogShown())
		{
			m_SelectedInstance = value;
			m_PropertyChanged(*this, PropertyChangedEventArgs(L"SelectedInstance"));
		}
	}

	event_token ShellViewModel::PropertyChanged(PropertyChangedEventHandler const& handler) noexcept
	{
		return m_PropertyChanged.add(handler);
	}

	void ShellViewModel::PropertyChanged(event_token token) noexcept
	{
		m_PropertyChanged.remove(token);
	}
}