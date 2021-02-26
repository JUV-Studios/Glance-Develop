#pragma once
#include "App.h"
#include "ShellViewModel.g.h"

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>
    {
        ShellViewModel();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        void AddStorageItems(Windows::Foundation::Collections::IVectorView<Windows::Storage::IStorageItem2> const& sItems);
        Windows::Foundation::IAsyncAction RemoveInstance(Develop::ShellView const view);
        uint32_t SelectedIndex();
        void SelectedIndex(uint32_t index);
        ObservableReferenceProperty(Develop::ShellView, SelectedInstance, m_Bindable);
        PropertyChangedHandler(m_Bindable);
    private:
        bool StorageItemOpen(Windows::Storage::IStorageItem2 const& item, ShellView* const foundItem);
        void AddInstances(std::vector<ShellView> const& instances);
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> m_Instances;
        const JUVStudios::BindableObject m_Bindable{ *this };
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
