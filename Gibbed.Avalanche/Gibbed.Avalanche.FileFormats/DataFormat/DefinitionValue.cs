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

using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats.DataFormat
{
    public class DefinitionValue
    {
        public string Name;
        public uint TypeHash;
        public uint Size;
        public uint Offset;
        public uint Unknown2C;
        public uint Unknown30;
        public uint Unknown34;

        public void Deserialize(Stream input, Endian endian)
        {
            this.Name = input.ReadString(32, true, Encoding.ASCII);
            this.TypeHash = input.ReadValueU32(endian);
            this.Size = input.ReadValueU32(endian);
            this.Offset = input.ReadValueU32(endian);
            this.Unknown2C = input.ReadValueU32(endian);
            this.Unknown30 = input.ReadValueU32(endian);
            this.Unknown34 = input.ReadValueU32(endian);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.Name) ? base.ToString() : this.Name;
        }
    }
}
