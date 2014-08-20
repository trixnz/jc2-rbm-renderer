#pragma once

class hkaBone;

namespace Havok{
	namespace Animation{
		public ref class HkaBone
		{
		internal:
			HkaBone(hkaBone* bone);

		public:
			property System::String^ Name{ System::String^ get(); }
			property bool LockTranslation{ bool get(); }

		private:
			hkaBone* nativeBone_;
		};
	}
}