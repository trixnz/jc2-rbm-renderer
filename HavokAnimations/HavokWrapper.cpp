#include "HavokWrapper.hpp"
#include "HkaAnimationContainer.hpp"

#pragma unmanaged
#define HK_CLASSES_FILE <Common/Serialize/Classlist/hkAnimationClasses.h>
#include <Common/Serialize/Util/hkBuiltinTypeRegistry.cxx>
#include <Common/Compat/hkCompat_None.cxx>

#include <Common/Base/Memory/hkThreadMemory.h>
#include <Common/Base/Memory/Memory/Pool/hkPoolMemory.h>

#pragma comment(lib, "hkBase")
#pragma comment(lib, "hkaInternal")
#pragma comment(lib, "hkaAnimation")
#pragma comment(lib, "hkSerialize")
#pragma comment(lib, "hkCompat")
#pragma comment(lib, "hkSceneData")
#pragma managed

namespace Havok{

static void HK_CALL errorReport(const char* msg, void* object)
{
	System::Diagnostics::Debug::WriteLine(gcnew String(msg));
}

Animation::HkaAnimationContainer^ HavokWrapper::LoadAnimationContainer(array<Byte>^ bytes)
{
	auto container = gcnew Animation::HkaAnimationContainer(bytes);

	return container;
}

HavokWrapper::HavokWrapper()
{
	auto poolMemory = new hkPoolMemory();
	auto threadMemory = new hkThreadMemory(poolMemory);

	hkBaseSystem::init(poolMemory, threadMemory, errorReport);
	poolMemory->removeReference();

	const int stackSize = 0x40000;
	char* stackBuffer = hkAllocate<char>(stackSize, HK_MEMORY_CLASS_BASE);
	threadMemory->setStackArea(stackBuffer, stackSize);
}

}