#pragma once
#include "ShellViewModel.g.h"

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>
    {
        ShellViewModel();
        static Develop::ShellViewModel Instance();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        Windows::Foundation::IAsyncAction AddStorageItems(Windows::Foundation::Collections::IVectorView<Windows::Storage::IStorageItem2> const& sItems);
        Windows::Foundation::IAsyncAction RemoveInstance(Develop::ShellView const& view);
        bool TryCloseInstance(Develop::ShellView const& view);
        void FastSwitch(bool reverse);
        Develop::ShellView SelectedInstance();
        void SelectedInstance(Develop::ShellView const& value);
        uint32_t SelectedIndex();
        void SelectedIndex(uint32_t index);
        event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler) noexcept;
        void PropertyChanged(event_token token) noexcept;
    private:
        void AddInstances(array_view<ShellView> const& instances);
        ShellView FindInstance(Windows::Storage::IStorageItem2 const& refSource);
        Develop::ShellView m_StartTab = nullptr;
        Develop::ShellView m_SelectedInstance = nullptr;
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> m_Instances = single_threaded_observable_vector<Develop::ShellView>();
        event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
