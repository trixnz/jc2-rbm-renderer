#include "HkaAnimationBinding.hpp"
#include "HkaSkeletalAnimation.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#include <Animation/Animation/Animation/hkaAnimationBinding.h>
#pragma managed

namespace Havok { namespace Animation {

HkaAnimationBinding::HkaAnimationBinding(hkaAnimationBinding* nativeAnimationBinding)
	: nativeAnimationBinding_(nativeAnimationBinding)
{
		
}

HkaSkeletalAnimation^ HkaAnimationBinding::Animation::get()
{
	return gcnew HkaSkeletalAnimation(nativeAnimationBinding_->m_animation);
}

}}