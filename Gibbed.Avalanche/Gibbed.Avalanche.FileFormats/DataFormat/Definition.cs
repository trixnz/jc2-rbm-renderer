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

using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats.DataFormat
{
    public class Definition
    {
        public DefinitionType DefinitionType;
        public uint Size;
        public uint Unknown08;
        public uint TypeHash;
        public string Name;
        public uint Unknown50;
        public uint ElementTypeHash;
        public uint Unknown58;
        public uint Unknown5C;
        public List<DefinitionValue> ValueDefinitions = new List<DefinitionValue>();

        public void Deserialize(Stream input, Endian endian)
        {
            this.DefinitionType = input.ReadValueEnum<DefinitionType>(endian);
            this.Size = input.ReadValueU32(endian);
            this.Unknown08 = input.ReadValueU32(endian);
            this.TypeHash = input.ReadValueU32(endian);
            this.Name = input.ReadString(64, true, Encoding.ASCII);
            this.Unknown50 = input.ReadValueU32(endian);
            this.ElementTypeHash = input.ReadValueU32(endian);
            this.Unknown58 = input.ReadValueU32(endian);

            uint count = input.ReadValueU32(endian);
            this.ValueDefinitions.Clear();
            for (uint i = 0; i < count; i++)
            {
                var definition = new DefinitionValue();
                definition.Deserialize(input, endian);
                this.ValueDefinitions.Add(definition);
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.Name) ? base.ToString() : this.Name;
        }
    }
}
