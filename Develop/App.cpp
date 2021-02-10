#include "pch.h"
#include "App.h"
#include "MainPage.h"
#include <winrt/DevelopManaged.h>

using namespace winrt;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Activation;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Navigation;
using namespace Develop;
using namespace Develop::implementation;

int __stdcall wWinMain(HINSTANCE, HINSTANCE, PWSTR, int)
{
    init_apartment();
    Application::Start([](auto&&)
        {
            make<Develop::implementation::App>();
            Develop::AppSettings::InitializeAsync();
        });

    return 0;
}

/// <summary>
/// Initializes the singleton application object.  This is the first line of authored code
/// executed, and as such is the logical equivalent of main() or WinMain().
/// </summary>
App::App()
{
    InitializeComponent();
    Suspending({ this, &App::OnSuspending });

#if defined _DEBUG && !defined DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
    UnhandledException([this](IInspectable const&, UnhandledExceptionEventArgs const& e)
    {
        if (IsDebuggerPresent())
        {
            auto errorMessage = e.Message();
            __debugbreak();
        }
    });
#endif
}

void App::OnLaunched(LaunchActivatedEventArgs const& e) { ActivateApp(e); }

void App::OnActivated(IActivatedEventArgs const& e) { ActivateApp(e); }

void App::OnFileActivated(FileActivatedEventArgs const& e) { ActivateApp(e); }

/// <summary>
/// Invoked when application execution is being suspended.  Application state is saved
/// without knowing whether the application will be terminated or resumed with the contents
/// of memory still intact.
/// </summary>
/// <param name="sender">The source of the suspend request.</param>
/// <param name="e">Details about the suspend request.</param>
void App::OnSuspending([[maybe_unused]] IInspectable const& sender, [[maybe_unused]] SuspendingEventArgs const& e)
{
    // Save application state and stop any background activity
}

fire_and_forget App::ActivateApp(IActivatedEventArgs const& args)
{
    Develop::MainPage mainPage{ nullptr };
    if (Window::Current().Content() == nullptr)
    {
        mainPage = make<MainPage>();
        Window::Current().Content(mainPage);
    }
    else mainPage = Window::Current().Content().as<Develop::MainPage>();

    FileActivatedEventArgs fileArgs{ nullptr };
    if (args.try_as(fileArgs)) mainPage.ViewModel().AddStorageItems(collection_view_as<IStorageItem2>(fileArgs.Files()));
    Window::Current().Activate();
    return fire_and_forget();
}