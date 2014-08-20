#include "HkaBone.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#include <Animation/Animation/Rig/hkaBone.h>
#pragma managed

using System::String;

namespace Havok{
	namespace Animation{

		HkaBone::HkaBone(hkaBone* bone)
			: nativeBone_(bone)
		{

		}

		String^ HkaBone::Name::get()
		{
			return gcnew String(nativeBone_->m_name);
		}

		bool HkaBone::LockTranslation::get()
		{
			return nativeBone_->m_lockTranslation;
		}
	}
}