#include "ShellViewModel.h"
#if __has_include("ShellViewModel.g.cpp")
#include "ShellViewModel.g.cpp"
#endif

using namespace winrt;
using namespace JUVStudios;
using namespace Windows::System;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::ApplicationModel::Core;

namespace winrt::Develop::implementation
{
	FontIconSource fileIcon = nullptr;

	ShellViewModel::ShellViewModel()
	{
		m_Instances = single_threaded_observable_vector<ShellView>();
		FontIconSource f;
		f.Glyph(L"\uE10F");
		f.FontFamily(FontFamily(L"Segoe MDL2 Assets"));
		auto startPageView = ShellView(Helpers::GetResourceTranslation(L"HomePage/Header"), HomePage(), f, nullptr);
		AddInstances({ &startPageView, 1 });
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

	IAsyncAction ShellViewModel::AddStorageItems(IVectorView<IStorageItem2> const& sItems)
	{
		std::vector<ShellView> viewsToAdd;
		for (auto&& item : sItems)
		{
			auto existingInstance = m_Instances.GetAt(FindSingle<ShellView>(m_Instances.GetView(), 0, [&](ShellView tabInstance)
				{
					if (tabInstance.ReferenceSource() != nullptr) return tabInstance.ReferenceSource().IsEqual(item);
					else return false;
				}));

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
				if (fileIcon == nullptr)
				{
					fileIcon = FontIconSource();
					fileIcon.Glyph(L"\uE130");
					fileIcon.FontFamily(FontFamily(L"Segoe MDL2 Assets"));
				}
				
				viewsToAdd.emplace_back(file.Name(), CodeEditor(file), fileIcon, file);
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
		if (selectedIndex == index) Windows::UI::Xaml::Window::Current().Content().as<MainPage>().GoStart();
		IAsyncClosable closable;
		if (view.Content().try_as(closable)) co_await closable.CloseAsync();
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

	IInspectable ShellViewModel::GetHolder() const noexcept { return *this; }
}