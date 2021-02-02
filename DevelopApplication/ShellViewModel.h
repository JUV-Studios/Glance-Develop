#pragma once

#include "ShellViewModel.g.h"
#include "ViewModelHelper.h"
#include <winrt/DevelopManaged.h>

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>
    {
        ShellViewModel();
        Windows::Foundation::Collections::IObservableVector<DevelopManaged::ShellView> Instances();
        DevelopManaged::ShellView SelectedInstance();
        void SelectedInstance(DevelopManaged::ShellView const& value);
        void AddInstances(Windows::Foundation::Collections::IVectorView<DevelopManaged::ShellView> views);
        event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(event_token const& token) noexcept;
    private:
        Windows::Foundation::Collections::IObservableVector<DevelopManaged::ShellView> m_Instances;
        DevelopManaged::ShellView m_SelectedInstance;
        ViewModelHelper m_ViewModel;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
