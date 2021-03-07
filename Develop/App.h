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
        fire_and_forget ActivateAppAsync(Windows::ApplicationModel::Activation::IActivatedEventArgs);
    };
}