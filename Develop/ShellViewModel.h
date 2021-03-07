#pragma once
#include "ShellViewModel.g.h"

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>, JUVStudios::MVVM::ViewModelBase
    {
        ShellViewModel();
        static Develop::ShellViewModel Instance();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        Windows::Foundation::IAsyncAction AddStorageItems(Windows::Foundation::Collections::IVectorView<Windows::Storage::IStorageItem2> const& sItems);
        Windows::Foundation::IAsyncAction RemoveInstance(Develop::ShellView const& view);
        bool TryCloseInstance(Develop::ShellView const& view);
        void FastSwitch(bool reverse);
        uint32_t SelectedIndex();
        void SelectedIndex(uint32_t index);
        JUVStudios::MVVM::ObservableProperty<Develop::ShellView> SelectedInstance { this, L"SelectedInstance", nullptr };
    protected:
        Windows::Foundation::IInspectable GetHolder() const noexcept override;
    private:
        void AddInstances(array_view<ShellView> const& instances);
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> m_Instances;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
