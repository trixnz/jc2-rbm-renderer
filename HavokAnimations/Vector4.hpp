#pragma once

namespace Havok{

public ref class Vector4
{
public:
	Vector4(float x, float y, float z)
		: Vector4(x, y, z, 0)
	{
		
	}

	Vector4(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	property float X;
	property float Y;
	property float Z;
	property float W;
};

}