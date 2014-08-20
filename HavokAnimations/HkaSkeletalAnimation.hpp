#pragma once

class hkaSkeletalAnimation;

namespace Havok {
	namespace Animation {

		public ref class HkaSkeletalAnimation
		{
		public:
			HkaSkeletalAnimation(hkaSkeletalAnimation* nativeSkeletalAnimation);

		private:
			hkaSkeletalAnimation* nativeAnimation_;
		};

	}
}