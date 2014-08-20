#pragma once

class hkaAnimationBinding;

namespace Havok { namespace Animation {

ref class HkaSkeletalAnimation;

public ref class HkaAnimationBinding
{
public:
	HkaAnimationBinding(hkaAnimationBinding* nativeAnimationBinding);

	property HkaSkeletalAnimation^ Animation { HkaSkeletalAnimation^ get(); }

internal:
	hkaAnimationBinding* nativeAnimationBinding_;
};

}}