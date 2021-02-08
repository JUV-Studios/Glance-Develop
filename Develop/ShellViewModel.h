#pragma once

#include "ShellViewModel.g.h"
#include "BindableBase.h"

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, BindableBase>
    {
        ShellViewModel();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        Develop::ShellView SelectedInstance();
        void SelectedInstance(Develop::ShellView const& value);
        void AddInstances(array_view<ShellView> instances);
        void AddStorageItems(Windows::Foundation::Collections::IVectorView<Windows::Storage::IStorageItem2> const& sItems);
    private:
        bool StorageItemOpen(Windows::Storage::IStorageItem2 const& item, ShellView* foundItem);
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> m_Instances;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
