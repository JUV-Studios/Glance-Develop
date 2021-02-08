#pragma once

#include "BindableBase.g.h"

namespace winrt::Develop::implementation
{
    struct BindableBase : BindableBaseT<BindableBase>
    {
        BindableBase() = default;
        void RaisePropertyChanged(hstring const& propertyName) noexcept;
        Windows::Foundation::IInspectable GetProperty(hstring const& propertyName) noexcept;
        void SetProperty(hstring const& propertyName, Windows::Foundation::IInspectable const& value);
        event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(event_token const& token) noexcept;
    private:
        Windows::Foundation::Collections::PropertySet m_Properties;
        event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_PropertyChanged;
    };
}

namespace winrt::Develop::factory_implementation
{
    struct BindableBase : BindableBaseT<BindableBase, implementation::BindableBase>
    {
    };
}
