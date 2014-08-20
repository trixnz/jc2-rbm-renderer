#include "HkaPose.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#include <Animation/Animation/Rig/hkaPose.h>
#pragma managed

using System::String;

namespace Havok{
	namespace Animation{

		HkaPose::HkaPose(hkaPose& pose)
		{
			auto& modelSpace = pose.accessPoseModelSpace();
			
			ModelSpace = gcnew array<HkQsTransform^>(modelSpace.getSize());
			for (int i = 0; i < modelSpace.getSize(); ++i)
				ModelSpace[i] = gcnew HkQsTransform(&modelSpace[i]);
		}
	}
}