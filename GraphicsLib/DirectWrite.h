#pragma once
namespace GraphicsLib
{
	public ref struct DirectWrite sealed
	{
		DirectWrite();
		property Windows::Foundation::Collections::IVectorView<Platform::String^>^ FontsList
		{
			Windows::Foundation::Collections::IVectorView<Platform::String^>^ get();
		}
	private:
		Platform::Collections::Vector<Platform::String^>^ m_FontsList;
	};
}