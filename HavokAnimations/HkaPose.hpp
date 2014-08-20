#pragma once

#include "HkQsTransform.hpp"

class hkaPose;

namespace Havok{
	namespace Animation{
		public ref class HkaPose
		{
		internal:
			HkaPose(hkaPose& pose);

		public:
			property array<HkQsTransform^>^ ModelSpace;
		};
	}
}