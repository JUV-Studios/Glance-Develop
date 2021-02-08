#include "pch.h"
#include "Recents.h"
#if __has_include("Recents.g.cpp")
#include "Recents.g.cpp"
#endif

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Storage;
using namespace Windows::Storage::AccessCache;

namespace winrt::Develop::implementation
{
	IObservableVector<DevelopManaged::RecentItem> recentItems;

	auto recentsList = StorageApplicationPermissions::MostRecentlyUsedList();

	auto timeContainer = ApplicationData::Current().LocalSettings().CreateContainer(L"RecentFileTime", ApplicationDataCreateDisposition::Always);

	std::mutex loadLock;

	IObservableVector<DevelopManaged::RecentItem> Recents::RecentItems()
	{
		if (recentItems == nullptr) recentItems = single_threaded_observable_vector<DevelopManaged::RecentItem>();
		return recentItems;
	}

	IAsyncAction Recents::LoadRecentsAsync()
	{
		if (loadLock.try_lock())
		{
			apartment_context callingThread;
			co_await resume_background();
			std::vector<DevelopManaged::RecentItem> tempItems;
			tempItems.reserve(recentsList.Entries().Size());
			for (auto const& entry : recentsList.Entries())
			{
				try
				{
					DateTime time;
					auto dtValue = timeContainer.Values().TryLookup(entry.Token).try_as<DateTime>();
					if (dtValue) time = dtValue.value();
					else time = clock::now();
					auto file = co_await recentsList.GetItemAsync(entry.Token);
					tempItems.push_back(DevelopManaged::RecentItem(file.as<IStorageItem2>(), time, entry));
				}
				catch (hresult_error const& ex)
				{
					if (ex.code() != HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND)) throw ex;
				}
			}

			std::sort(tempItems.begin(), tempItems.end(), [](auto const& first, auto const& second)
				{
					return first.Time().time_since_epoch() < second.Time().time_since_epoch();
				});

			co_await callingThread;
			RecentItems().ReplaceAll(std::move(tempItems));
			loadLock.unlock();
		}
	}
}
