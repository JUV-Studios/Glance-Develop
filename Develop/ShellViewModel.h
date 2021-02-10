#pragma once
#include "ShellViewModel.g.h"
#include <winrt/JUVStudios.h>

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>
    {
        ShellViewModel();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        Develop::ShellView SelectedInstance();
        void SelectedInstance(Develop::ShellView const& value);
        void AddInstances(array_view<ShellView> instances);
        void AddStorageItems(Windows::Foundation::Collections::IVectorView<Windows::Storage::IStorageItem2> const& sItems);
        inline event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler) const { return m_Bindable.PropertyChanged(handler); }
        inline void PropertyChanged(winrt::event_token const& token) const noexcept { m_Bindable.PropertyChanged(token); }
    private:
        bool StorageItemOpen(Windows::Storage::IStorageItem2 const& item, ShellView* foundItem);
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
