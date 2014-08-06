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
    public class SkinnedGeneralSkinBatch : IFormat
    {
        public int FaceCount;
        public int FaceIndex;
        public List<short> BoneIndices = new List<short>();

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this.FaceCount = input.ReadValueS32(endian);
            this.FaceIndex = input.ReadValueS32(endian);

            var count = input.ReadValueS32(endian);
            this.BoneIndices.Clear();
            this.BoneIndices.Capacity = count;
            for (int i = 0; i < count; i++)
            {
                this.BoneIndices.Add(input.ReadValueS16(endian));
            }
        }
    }
}
