#pragma once
#include "ShellViewModel.g.h"
#include <winrt/JUVStudios.h>
#include <JUVStudios/cppwinrtHelpers.h>

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>
    {
        ShellViewModel();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        void AddInstances(std::vector<ShellView> const& instances);
        void AddStorageItems(Windows::Foundation::Collections::IVectorView<Windows::Storage::IStorageItem2> const& sItems);
        ObservableReferenceProperty(Develop::ShellView, SelectedInstance, m_Bindable);
        PropertyChangedHandler(m_Bindable);
    private:
        bool StorageItemOpen(Windows::Storage::IStorageItem2 const& item, ShellView& foundItem);
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> m_Instances;
        JUVStudios::BindableObject m_Bindable{ *this };
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
