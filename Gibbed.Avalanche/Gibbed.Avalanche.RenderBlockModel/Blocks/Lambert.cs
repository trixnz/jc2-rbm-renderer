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
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel.Blocks
{
    public class Lambert : IRenderBlock
    {
        public uint Unknown28;
        public Material Material;
        public readonly List<LambertData0Small> VertexData0Small = new List<LambertData0Small>();
        public readonly List<LambertData0Big> VertexData0Big = new List<LambertData0Big>();
        public readonly List<short> Faces = new List<short>();

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            var version = input.ReadValueU8();
            if (version != 4)
            {
                throw new FormatException();
            }

            if (version == 4)
            {
                this.Unknown28 = input.ReadValueU32(endian);
                input.ReadBytes(40);
            }
            else
            {
                throw new NotSupportedException();
            }

            this.Material.Deserialize(input, endian);

            if (this.Unknown28 == 0)
            {
                input.ReadArray(this.VertexData0Big, endian);
            }
            else if (this.Unknown28 == 1)
            {
                input.ReadArray(this.VertexData0Small, endian);
            }

            input.ReadFaces(this.Faces, endian);
        }
    }
}
