#include "Animator.hpp"
#include "HkaSkeleton.hpp"
#include "HkaAnimationBinding.hpp"
#include "HkaPose.hpp"

#pragma unmanaged
#include <Animation/Animation/Playback/hkaAnimatedSkeleton.h>
#include <Animation/Animation/Playback/Control/Default/hkaDefaultAnimationControl.h>
#include <Animation/Animation/Rig/hkaPose.h>
#pragma managed

namespace Havok { namespace Animation {

	Animator::Animator(HkaSkeleton^ skeleton, HkaAnimationBinding^ binding)
		: skeleton_(skeleton), binding_(binding)
	{
		nativeAnimationControl_ = new hkaDefaultAnimationControl(binding_->nativeAnimationBinding_);
		
		nativeAnimatedSkeleton_ = new hkaAnimatedSkeleton(skeleton_->nativeSkeleton_);
		nativeAnimatedSkeleton_->addAnimationControl(nativeAnimationControl_);
	}

	HkaPose^ Animator::Step(float deltaTime)
	{
		nativeAnimatedSkeleton_->stepDeltaTime(deltaTime);

		hkaPose pose(skeleton_->nativeSkeleton_);
		pose.setToReferencePose();

		nativeAnimatedSkeleton_->sampleAndCombineAnimations(
			pose.writeAccessPoseLocalSpace().begin(),
			pose.writeAccessFloatSlotValues().end());
		
		return gcnew HkaPose(pose);
	}

}}