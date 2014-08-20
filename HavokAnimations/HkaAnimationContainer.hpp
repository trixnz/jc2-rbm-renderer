#pragma once

using namespace System;

class hkLoader;
class hkRootLevelContainer;
class hkaAnimationContainer;

namespace Havok
{
	namespace Animation
	{
		ref class HkaSkeleton;
		ref class HkaAnimationBinding;

		public ref class HkaAnimationContainer
		{
		public:
			HkaAnimationContainer(array<Byte>^ data);
			~HkaAnimationContainer();

			property int NumSkeletons { int get(); };
			property int NumAnimations { int get(); };
			property int NumBindings { int get(); }

			HkaSkeleton^ GetSkeleton(int index);
			HkaAnimationBinding^ GetAnimationBinding(int index);

		private:
			hkLoader* loader_;
			hkRootLevelContainer* rootContainer_;
			hkaAnimationContainer* animationContainer_;
		};
	}
}