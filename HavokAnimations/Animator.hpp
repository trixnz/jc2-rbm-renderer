#pragma once

class hkaAnimatedSkeleton;
class hkaDefaultAnimationControl;

namespace Havok { namespace Animation {
ref class HkaSkeleton;
ref class HkaAnimationBinding;
ref class HkaPose;

public ref class Animator
{
public:
	Animator(HkaSkeleton^ skeleton, HkaAnimationBinding^ binding);

	HkaPose^ Step(float deltaTime);

private:
	HkaSkeleton^ skeleton_;
	HkaAnimationBinding^ binding_;

	hkaAnimatedSkeleton* nativeAnimatedSkeleton_;
	hkaDefaultAnimationControl* nativeAnimationControl_;
	
	array<hkaDefaultAnimationControl*>^ nativeControls_;
};
}}