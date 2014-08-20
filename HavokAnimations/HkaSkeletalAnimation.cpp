#include "HkaSkeletalAnimation.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#include <Animation/Animation/Animation/hkaSkeletalAnimation.h>
#pragma managed

namespace Havok {
	namespace Animation {

		HkaSkeletalAnimation::HkaSkeletalAnimation(hkaSkeletalAnimation* nativeAnimation)
			: nativeAnimation_(nativeAnimation)
		{
		}

	}
}