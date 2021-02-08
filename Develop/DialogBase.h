#pragma once
#include "pch.h"

namespace JUVStudios
{
	struct DialogPresentation
	{
        bool await_ready() const noexcept
        {
            return false;
        }

        void await_resume() const noexcept
        {
        }

        void await_suspend(impl::coroutine_handle<> handle) const
        {
            auto copy = context; // resuming may destruct *this, so use a copy
            impl::resume_apartment(copy, handle);
        }

        impl::resume_apartment_context context;
	};
}