using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel
{
	public enum Packing
	{
		XZY,
		ZXY,
		XYZ,
		Colour
	}

	public struct PackedVector
	{
		public PackedVector(Packing packing, Stream input) : this()
		{
			Packing = packing;

			Deserialize(input);
		}

		public Packing Packing { get; set; }

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public void Deserialize(Stream stream)
		{
			float packed = stream.ReadValueF32();

			switch (Packing)
			{
				case Packing.XZY:
					X = packed;
					Y = packed/65536.0f;
					Z = packed/256.0f;
					break;

				case Packing.ZXY:
					X = packed/256.0f;
					Y = packed/65536.0f;
					Z = packed;
					break;

				case Packing.XYZ:
					X = packed;
					Y = packed/256.0f;
					Z = packed/65536.0f;
					break;

				case Packing.Colour:
					X = packed;
					Y = packed/64;
					Z = packed/4096;
					break;
			}

			X -= (float) Math.Floor(X);
			Y -= (float) Math.Floor(Y);
			Z -= (float) Math.Floor(Z);

			if (Packing != Packing.Colour)
			{
				X = X*2 - 1;
				Y = Y*2 - 1;
				Z = Z*2 - 1;
			}
		}
	}

	public struct PackedFloat
	{
		public PackedFloat(short packed) : this()
		{
			Value = GetFloatFromS16N(packed);
		}

		public PackedFloat(Stream input)
			: this(input.ReadValueS16())
		{
		}

		public float Value { get; set; }

		public static implicit operator float(PackedFloat p)
		{
			return p.Value;
		}

		private static float GetFloatFromS16N(short c)
		{
			return c*(1.0f/32767);
		}
	}

	public struct PackedByteVector
	{
		public PackedByteVector(byte[] packed)
			: this()
		{
			X = GetFloatFromB8U(packed[0]);
			Y = GetFloatFromB8U(packed[1]);
			Z = GetFloatFromB8U(packed[2]);
			W = GetFloatFromB8U(packed[3]);
		}

		public PackedByteVector(Stream input)
			: this(input.ReadBytes(4))
		{
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float W { get; set; }
		public bool Clamp { get; set; }

		private float GetFloatFromB8U(byte c)
		{
			float ret = c*(1.0f/255.0f);

			if (Clamp)
				ret = (ret + 2) - 1;

			return ret;
		}
	}
}