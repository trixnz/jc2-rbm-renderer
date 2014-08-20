#include "HkaSkeleton.hpp"
#include "HkaBone.hpp"

#pragma unmanaged
#include <Common/Base/hkBase.h>
#include <Animation/Animation/Rig/hkaSkeleton.h>
#pragma managed

using System::String;

namespace Havok{namespace Animation{

	HkaSkeleton::HkaSkeleton(hkaSkeleton* skeleton)
		: nativeSkeleton_(skeleton)
	{

	}

	String^ HkaSkeleton::Name::get()
	{
		return gcnew String(nativeSkeleton_->m_name);
	}

	int HkaSkeleton::NumBones::get()
	{
		return nativeSkeleton_->m_numBones;
	}

	int HkaSkeleton::NumParentIndices::get()
	{
		return nativeSkeleton_->m_numParentIndices;
	}

	int HkaSkeleton::NumReferencePose::get()
	{
		return nativeSkeleton_->m_numReferencePose;
	}

	array<HkaBone^>^ HkaSkeleton::Bones::get()
	{
		auto ret = gcnew array<HkaBone^>(NumBones);

		for (int i = 0; i < NumBones; ++i)
			ret[i] = gcnew HkaBone(nativeSkeleton_->m_bones[i]);

		return ret;
	}

	array<short>^ HkaSkeleton::ParentIndices::get()
	{
		auto ret = gcnew array<short>(NumParentIndices);

		for (int i = 0; i < NumParentIndices; ++i)
			ret[i] = nativeSkeleton_->m_parentIndices[i];

		return ret;
	}

	array<HkQsTransform^>^ HkaSkeleton::ReferencePose::get()
	{
		auto ret = gcnew array<HkQsTransform^>(NumReferencePose);

		for (int i = 0; i < NumReferencePose; ++i)
			ret[i] = gcnew HkQsTransform(&nativeSkeleton_->m_referencePose[i]);

		return ret;
	}
}}