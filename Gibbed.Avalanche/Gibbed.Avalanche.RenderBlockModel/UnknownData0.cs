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
using Gibbed.Avalanche.FileFormats;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel
{
    public struct UnknownData0 : IFormat
    {
        public float ColorTone1R;
        public float ColorTone1G;
        public float ColorTone1B;
        public float ColorTone2R;
        public float ColorTone2G;
        public float ColorTone2B;
        public float Unknown18;
        public float DepthMaybe;
        public float Shininess;
        public float Unknown24;
        public float Unknown28;
        public float DirtRatioMaybe;
        public float Unknown30;
        public uint Unknown34;

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this.ColorTone1R = input.ReadValueF32(endian);
            this.ColorTone1G = input.ReadValueF32(endian);
            this.ColorTone1B = input.ReadValueF32(endian);
            this.ColorTone2R = input.ReadValueF32(endian);
            this.ColorTone2G = input.ReadValueF32(endian);
            this.ColorTone2B = input.ReadValueF32(endian);
            this.Unknown18 = input.ReadValueF32(endian);
            this.DepthMaybe = input.ReadValueF32(endian);
            this.Shininess = input.ReadValueF32(endian);
            this.Unknown24 = input.ReadValueF32(endian);
            this.Unknown28 = input.ReadValueF32(endian);
            this.DirtRatioMaybe = input.ReadValueF32(endian);
            this.Unknown30 = input.ReadValueF32(endian);
            this.Unknown34 = input.ReadValueU32(endian);
        }
    }
}
