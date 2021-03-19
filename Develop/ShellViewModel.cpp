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
		AddInstances({ &m_StartTab, 1 });
	}

	Develop::ShellViewModel ShellViewModel::Instance()
	{
		if (!CoreApplication::Properties().HasKey(L"Develop_ShellViewModel")) CoreApplication::Properties().Insert(L"Develop_ShellViewModel", make<ShellViewModel>());
		return CoreApplication::Properties().Lookup(L"Develop_ShellViewModel").as<Develop::ShellViewModel>();
	}

	IObservableVector<ShellView> ShellViewModel::Instances() { return m_Instances; }

	void ShellViewModel::AddInstances(array_view<ShellView> const& instances)
	{
		for (auto&& tabInstance : instances) m_Instances.Append(tabInstance);
		SelectedIndex(m_Instances.Size() - 1);
	}

	ShellView ShellViewModel::FindInstance(IStorageItem2 const& refSource)
	{
		for (auto&& tabInstance : m_Instances)
		{
			if (tabInstance.ReferenceSource() != nullptr)
			{
				if (tabInstance.ReferenceSource().IsEqual(refSource)) return tabInstance;
			}
		}

		return nullptr;
	}

	IAsyncAction ShellViewModel::AddStorageItems(IVectorView<IStorageItem2> const& sItems)
	{
		std::vector<ShellView> viewsToAdd;
		for (auto&& item : sItems)
		{
			auto existingInstance = FindInstance(item);
			if (existingInstance != nullptr)
			{
				if (sItems.Size() == 1 && !AppSettings::DialogShown())
				{
					SelectedInstance(existingInstance);
					return;
				}
			}
			else if (item.IsOfType(StorageItemTypes::File))
			{
				StorageFile file = item.as<StorageFile>();
				viewsToAdd.emplace_back(file.Name(), CodeEditor(file), FileIcon(), file);
			}
			else co_await Launcher::LaunchFolderAsync(item.as<StorageFolder>());
		}

		if (viewsToAdd.size() > 0) AddInstances(viewsToAdd);
		else SelectedIndex(m_Instances.Size() - 1);
	}

	IAsyncAction ShellViewModel::RemoveInstance(ShellView const& view)
	{
		uint32_t index = 0;
		auto selectedIndex = SelectedIndex();
		m_Instances.IndexOf(view, index);
		m_Instances.RemoveAt(index);
		if (selectedIndex == index) SelectedInstance(m_StartTab);
		IAsyncClosable closable;
		if (view.Content().try_as(closable)) co_await closable.CloseAsync();
		view.Close();
	}

	bool ShellViewModel::TryCloseInstance(ShellView const& view)
	{
		IAsyncClosable closable;
		if (view.Content().try_as<IAsyncClosable>(closable))
		{
			if (closable.StartClosing())
			{
				RemoveInstance(view);
				return true;
			}
		}

		return false;
	}

	void ShellViewModel::FastSwitch(bool reverse)
	{
		uint32_t index = SelectedIndex();
		if (m_Instances.Size() != 1)
		{
			if (reverse)
			{
				if (index == 0) SelectedIndex(m_Instances.Size() - 1);
				else SelectedIndex(index - 1);
			}
			else
			{
				if (index == m_Instances.Size() - 1) SelectedIndex(0);
				else SelectedIndex(index + 1);
			}
		}
	}

	ShellView ShellViewModel::SelectedInstance()
	{
		return m_SelectedInstance;
	}

	void ShellViewModel::SelectedInstance(ShellView const& value)
	{
		if (m_SelectedInstance != value)
		{
			m_SelectedInstance = value;
			m_PropertyChanged(*this, PropertyChangedEventArgs(L"SelectedInstance"));
		}
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

	event_token ShellViewModel::PropertyChanged(PropertyChangedEventHandler const& handler) noexcept
	{
		return m_PropertyChanged.add(handler);
	}

	void ShellViewModel::PropertyChanged(event_token token) noexcept
	{
		m_PropertyChanged.remove(token);
	}
}