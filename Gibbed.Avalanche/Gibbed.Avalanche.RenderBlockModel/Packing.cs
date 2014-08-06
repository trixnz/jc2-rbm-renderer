using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel
{
	public enum Packing
	{
		XZY,
		ZXY,
		XYZ
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
			}

			X -= (float) Math.Floor(X);
			Y -= (float) Math.Floor(Y);
			Z -= (float) Math.Floor(Z);

			X = X*2 - 1;
			Y = Y*2 - 1;
			Z = Z*2 - 1;
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
			if (c == -1)
				return -1.0f;

			return c*(1.0f/32767);
		}
	}
}