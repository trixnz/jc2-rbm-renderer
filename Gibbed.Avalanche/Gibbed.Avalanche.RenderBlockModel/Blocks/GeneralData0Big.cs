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
using System.Collections.Generic;
using System.IO;
using Gibbed.Avalanche.FileFormats;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel.Blocks
{
    public struct GeneralData0Big : IFormat
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float TexCoord1A;
        public float TexCoord1B;
        public float TexCoord1C;
        public float TexCoord1D;
        public float TexCoord2A;
        public float TexCoord2B;
        public float TexCoord2C;

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this.PositionX = input.ReadValueF32(endian);
            this.PositionY = input.ReadValueF32(endian);
            this.PositionZ = input.ReadValueF32(endian);
            this.TexCoord1A = input.ReadValueF32(endian);
            this.TexCoord1B = input.ReadValueF32(endian);
            this.TexCoord1C = input.ReadValueF32(endian);
            this.TexCoord1D = input.ReadValueF32(endian);
            this.TexCoord2A = input.ReadValueF32(endian);
            this.TexCoord2B = input.ReadValueF32(endian);
            this.TexCoord2C = input.ReadValueF32(endian);
        }
    }
}
