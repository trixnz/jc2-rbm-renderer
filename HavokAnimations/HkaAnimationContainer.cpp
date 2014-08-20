#include "hkaAnimationContainer.hpp"
#include "hkaSkeleton.hpp"
#include "HkaAnimationBinding.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#include <Common/Base/System/Io/Reader/Memory/hkMemoryStreamReader.h>

#include <Common/Serialize/Util/hkLoader.h>
#include <Common/Serialize/Util/hkRootLevelContainer.h>
#include <Animation/Animation/hkaAnimationContainer.h>

#include <string>

#pragma managed
#include <vcclr.h>

using namespace System::Diagnostics;

namespace Havok { namespace Animation{


HkaAnimationContainer::HkaAnimationContainer(array<Byte>^ data)
{
	cli::pin_ptr<unsigned char> arrayPtr(&data[0]);
	hkMemoryStreamReader reader(arrayPtr, data->Length, hkMemoryStreamReader::MEMORY_INPLACE);

	loader_ = new hkLoader();
	rootContainer_ = loader_->load(&reader);

	animationContainer_ = reinterpret_cast<hkaAnimationContainer*>(
		rootContainer_->findObjectByType(hkaAnimationContainerClass.getName()));
}

HkaAnimationContainer::~HkaAnimationContainer()
{
	delete loader_;
}

HkaSkeleton^ HkaAnimationContainer::GetSkeleton(int index)
{
	if (index < 0 || index >= NumSkeletons)
		return nullptr;

	return gcnew HkaSkeleton(animationContainer_->m_skeletons[index]);
}

HkaAnimationBinding^ HkaAnimationContainer::GetAnimationBinding(int index)
{
	if (index < 0 || index >= NumBindings)
		return nullptr;

	return gcnew HkaAnimationBinding(animationContainer_->m_bindings[index]);
}


int HkaAnimationContainer::NumSkeletons::get()
{
	return animationContainer_->m_numSkeletons;
}

int HkaAnimationContainer::NumAnimations::get()
{
	return animationContainer_->m_numAnimations;
}

int HkaAnimationContainer::NumBindings::get()
{
	return animationContainer_->m_numBindings;
}

}
}