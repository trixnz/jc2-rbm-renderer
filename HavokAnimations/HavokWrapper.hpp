#pragma once

namespace Havok{
namespace Animation{
	ref class HkaAnimationContainer;
}

public ref class HavokWrapper
{
public:
	HavokWrapper();

	Animation::HkaAnimationContainer^ LoadAnimationContainer(array<System::Byte>^ bytes);

private:
};
}