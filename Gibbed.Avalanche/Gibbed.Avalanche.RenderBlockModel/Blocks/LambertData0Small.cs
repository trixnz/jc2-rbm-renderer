/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel.Blocks
{
    public struct LambertData0Small : IFormat
    {
		public PackedFloat U;
		public PackedFloat V;
        public short TexCoord1C;
        public short TexCoord1D;
        public float TexCoord2A;
        public float TexCoord2B;
        public float TexCoord2C;
        public PackedFloat PositionX;
        public PackedFloat PositionY;
        public PackedFloat PositionZ;
        public PackedFloat PositionW;

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this.U = new PackedFloat(input);
            this.V = new PackedFloat(input);
            this.TexCoord1C = input.ReadValueS16(endian);
            this.TexCoord1D = input.ReadValueS16(endian);
            this.TexCoord2A = input.ReadValueF32(endian);
            this.TexCoord2B = input.ReadValueF32(endian);
            this.TexCoord2C = input.ReadValueF32(endian);
            this.PositionX = new PackedFloat(input);
            this.PositionY = new PackedFloat(input);
            this.PositionZ = new PackedFloat(input);
            this.PositionW = new PackedFloat(input);
        }
    }
}
