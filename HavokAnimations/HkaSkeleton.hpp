#pragma once

#include "HkQsTransform.hpp"

class hkaSkeleton;

namespace Havok{ namespace Animation{
	ref class HkaBone;

	public ref class HkaSkeleton
	{
	internal:
		HkaSkeleton(hkaSkeleton* skeleton);

	public:
		property System::String^ Name{ System::String^ get(); }

		property int NumBones{ int get(); }
		property int NumParentIndices { int get(); }
		property int NumReferencePose { int get(); }

		property array<HkaBone^>^ Bones { array<HkaBone^>^ get(); }
		property array<short>^ ParentIndices { array<short>^ get(); }
		property array<HkQsTransform^>^ ReferencePose { array<HkQsTransform^>^ get(); }

	internal:
		hkaSkeleton* nativeSkeleton_;
	};
}}