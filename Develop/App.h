#pragma once
#include "App.xaml.g.h"

namespace winrt::Develop::implementation
{
    struct App : AppT<App>
    {
        App();
        void OnLaunched(Windows::ApplicationModel::Activation::LaunchActivatedEventArgs const&);
        void OnActivated(Windows::ApplicationModel::Activation::IActivatedEventArgs const&);
        void OnFileActivated(Windows::ApplicationModel::Activation::FileActivatedEventArgs const&);
        void OnSuspending(IInspectable const&, Windows::ApplicationModel::SuspendingEventArgs const&);
    private:
        fire_and_forget ActivateApp(Windows::ApplicationModel::Activation::IActivatedEventArgs const&);
    };
}

template<typename D, typename T>
winrt::com_ptr<D> as_self(T&& o)
{
    winrt::com_ptr<D> result;
    if constexpr (std::is_same_v<std::remove_reference_t<T>,
        winrt::Windows::Foundation::IInspectable>)
    {
        auto temp = o.as<winrt::default_interface<D>>();
        result.attach(winrt::get_self<D>(temp));
        winrt::detach_abi(temp);
    }
    else if constexpr (std::is_rvalue_reference_v<T&&>)
    {
        result.attach(winrt::get_self<D>(o));
        winrt::detach_abi(o);
    }
    else result.copy_from(winrt::get_self<D>(o));
    return result;
}

template <class To, class From, template <class> class ICollection>
auto collection_as(ICollection<From> from)
{
    static_assert(winrt::impl::has_category_v<From>, "From must be WinRT type.");
    static_assert(winrt::impl::has_category_v<To>, "To must be WinRT type.");
    std::vector<To> vector;
    vector.reserve(from.Size());
    for (auto&& item : from) vector.push_back(item.as<To>());
    return winrt::single_threaded_vector(std::move(vector));
}

template <class To, class From>
inline auto collection_view_as(winrt::Windows::Foundation::Collections::IVectorView<From> const& from) noexcept
{
    auto vector = collection_as<To, From>(from);
    return vector.GetView();
}