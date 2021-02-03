#pragma once

#include "ShellViewModel.g.h"
#include <winrt/DevelopManaged.h>

namespace winrt::Develop::implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel>
    {
        ShellViewModel();
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> Instances();
        Develop::ShellView SelectedInstance();
        void SelectedInstance(Develop::ShellView const& value);
        void AddInstances(Windows::Foundation::Collections::IVectorView<Develop::ShellView> views);
        winrt::event_token PropertyChanged(winrt::Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(winrt::event_token const& token) noexcept;
    private:
        Windows::Foundation::Collections::IObservableVector<Develop::ShellView> m_Instances;
        Develop::ShellView m_SelectedItem{ nullptr };
        event<winrt::Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct ShellViewModel : ShellViewModelT<ShellViewModel, implementation::ShellViewModel>
    {
    };
}
