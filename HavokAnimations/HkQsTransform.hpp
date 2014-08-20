#pragma once

#include "Vector4.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#pragma managed

namespace Havok{

public ref class HkQsTransform
{
public:
	HkQsTransform(hkQsTransform const* nativeTransform)
		: nativeTransform_(nativeTransform)
	{
		
	}

	property Vector4^ Translation {
		Vector4^ get() {
			hkVector4 const& translation = nativeTransform_->m_translation;

			return gcnew Vector4(translation(0), translation(1), translation(2));
		}
	}
	property Vector4^ Rotation {
		Vector4^ get() {
			hkQuaternion const& rotation = nativeTransform_->m_rotation;

			return gcnew Vector4(rotation(0), rotation(1), rotation(2), 
				rotation(3));
		}
	}

	property Vector4^ Scale {
		Vector4^ get() {
			hkVector4 const& scale = nativeTransform_->m_scale;

			return gcnew Vector4(scale(0), scale(1), scale(2));
		}
	}

private:
	hkQsTransform const* nativeTransform_;
};

}