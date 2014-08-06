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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats
{
    public class SmallArchiveFile
    {
        public class Entry
        {
            public string Name;
            public uint Offset;
            public uint Size;

            public void Serialize(Stream output, Endian endian)
            {
                output.WriteValueS32(Encoding.ASCII.GetByteCount(this.Name), endian);
                output.WriteString(this.Name, Encoding.ASCII);
                output.WriteValueU32(this.Offset, endian);
                output.WriteValueU32(this.Size, endian);
            }

            public void Deserialize(Stream input, Endian endian)
            {
                uint length = input.ReadValueU32(endian);
                if (length > 1024)
                {
                    throw new FormatException("doubt there is a file with more than 1024 characters in its name");
                }

                this.Name = input.ReadString(length, true, Encoding.ASCII);
                this.Offset = input.ReadValueU32(endian);
                this.Size = input.ReadValueU32(endian);
            }
        }

        public Endian Endian;
        public List<Entry> Entries = new List<Entry>();

        public void Serialize(Stream output)
        {
            var endian = this.Endian;

            using (var index = new MemoryStream())
            {
                foreach (var entry in this.Entries)
                {
                    entry.Serialize(index, endian);
                }

                index.SetLength(index.Length.Align(16));
                index.Position = 0;

                output.WriteValueU32(4, endian);
                output.WriteString("SARC", Encoding.ASCII);
                output.WriteValueU32(2, endian);
                output.WriteValueU32((uint)index.Length, endian);

                output.WriteFromStream(index, index.Length);
            }
        }

        public void Deserialize(Stream input)
        {
            uint magicSize = input.ReadValueU32();

            if (magicSize != 4 && magicSize.Swap() != 4)
            {
                throw new FormatException("bad header size");
            }

            var endian = magicSize == 4
                             ? Endian.Little
                             : Endian.Big;

            if (input.ReadString(4, Encoding.ASCII) != "SARC")
            {
                throw new FormatException("bad header magic");
            }

            uint version = input.ReadValueU32(endian);
            if (version != 1 && version != 2)
            {
                throw new FormatException("bad header version");
            }

            using (var index = input.ReadToMemoryStream(input.ReadValueU32(endian)))
            {
                this.Entries = new List<Entry>();
                while (index.Length - index.Position > 15)
                {
                    var entry = new Entry();
                    entry.Deserialize(index, endian);
                    this.Entries.Add(entry);
                }
            }

            this.Endian = endian;
        }
    }
}
