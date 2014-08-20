﻿/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel.Blocks
{
	public struct GeneralData0Small : IFormat
	{
		public PackedFloat PositionW;
		public PackedFloat PositionX;
		public PackedFloat PositionY;
		public PackedFloat PositionZ;
		public PackedFloat U;
		public PackedFloat V;
		public PackedFloat U2;
		public PackedFloat V2;
		public float TexCoord2A;
		public float TexCoord2B;
		public float TexCoord2C;

		public PackedVector UnkPacked3 { get; set; }
		public PackedVector UnkPacked2 { get; set; }
		public PackedVector Normal { get; set; }

		public void Serialize(Stream output, Endian endian)
		{
			throw new NotImplementedException();
		}

		public void Deserialize(Stream input, Endian endian)
		{
			U = new PackedFloat(input);
			V = new PackedFloat(input);
			U2 = new PackedFloat(input);
			V2 = new PackedFloat(input);

			Normal = new PackedVector(Packing.XYZ, input);
			UnkPacked2 = new PackedVector(Packing.XYZ, input);
			UnkPacked3 = new PackedVector(Packing.Colour, input);

			PositionX = new PackedFloat(input);
			PositionY = new PackedFloat(input);
			PositionZ = new PackedFloat(input);
			PositionW = new PackedFloat(input);
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2}",
				PositionX,
				PositionY,
				PositionZ);
		}
	}
}