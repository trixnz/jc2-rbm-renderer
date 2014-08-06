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
using System.Linq;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats
{
    public class SaveFile
    {
        public uint Chaos;
        public uint Money;
        public uint Completion;
        public uint GameTimeInSeconds;

        public List<Save.Section> Sections = new List<Save.Section>();

        public void Serialize(Stream output)
        {
            var endian = Endian.Little;

            var infos = new List<SectionInfo>();

            using (var temp = new MemoryStream())
            {
                using (var data = new MemoryStream())
                {
                    foreach (var section in this.Sections.OrderBy(s => s.ObjectId))
                    {
                        var start = data.Position;

                        foreach (var entry in section.Entries)
                        {
                            data.WriteValueS16(entry.Key, endian);

                            if (entry.Value == null)
                            {
                                data.WriteValueU32(0, endian);
                            }
                            else
                            {
                                data.WriteValueS32(entry.Value.Length, endian);
                                data.WriteBytes(entry.Value);
                            }
                        }

                        var end = data.Position;

                        var info = new SectionInfo();
                        info.ObjectId = section.ObjectId;
                        info.InstanceId = section.InstanceId;
                        info.Offset = (uint)start;
                        info.Size = (uint)(end - start);
                        infos.Add(info);
                    }

                    temp.SetLength(891904);

                    temp.Seek(0x00000004, SeekOrigin.Begin);
                    temp.WriteValueU32(this.Chaos, endian);
                    temp.WriteValueU32(this.Money, endian);
                    temp.WriteValueU32(this.Completion, endian);
                    temp.WriteValueU32(this.GameTimeInSeconds, endian);

                    temp.Seek(0x0000002C, SeekOrigin.Begin);
                    temp.WriteValueB8(true);

                    temp.Seek(0x00000030, SeekOrigin.Begin);
                    temp.WriteValueU32(12, endian);

                    temp.Seek(0x00000200, SeekOrigin.Begin);
                    temp.WriteValueS32(this.Sections.Count, endian);

                    foreach (var info in infos)
                    {
                        info.Serialize(temp, endian);
                    }

                    data.Flush();
                    data.Position = 0;

                    temp.Seek(0x00039A00, SeekOrigin.Begin);
                    temp.WriteFromStream(data, data.Length);
                }

                temp.Flush();
                temp.Position = 0;

                output.WriteFromStream(temp, temp.Length);
            }
        }

        public void Deserialize(Stream input)
        {
            var endian = Endian.Little;

            if (input.Length != 891904)
            {
                throw new FormatException("unexpected save size");
            }

            input.Seek(0x00000030, SeekOrigin.Begin);

            var version = input.ReadValueU32(endian);
            if (version != 12)
            {
                throw new FormatException("unexpected save version");
            }

            input.Seek(0x00000200, SeekOrigin.Begin);

            var count = input.ReadValueU32(endian);
            if (count * 16 > 0x00039800)
            {
                throw new FormatException("invalid save section count");
            }

            var infos = new SectionInfo[count];
            for (uint i = 0; i < count; i++)
            {
                var section = new SectionInfo();
                section.Deserialize(input, endian);

                if (section.Offset > 0x000A0000)
                {
                    throw new FormatException("invalid save section offset");
                }

                if (section.Offset + section.Size > 0x000A0000)
                {
                    throw new FormatException("invalid save section size");
                }

                infos[i] = section;
            }

            this.Sections.Clear();
            this.Sections.Capacity = (int)count;
            foreach (var info in infos)
            {
                var section = new Save.Section(info.ObjectId, info.InstanceId);

                long start = 0x00039A00 + info.Offset;
                long end = start + info.Size;

                input.Seek(start, SeekOrigin.Begin);
                while (input.Position < end)
                {
                    var index = input.ReadValueS16(endian);

                    if (section.Contains(index) == true)
                    {
                        throw new FormatException(string.Format("duplicate value {0} in section {1}", index, info));
                    }

                    int length = input.ReadValueS32();
                    if (input.Position + length > end)
                    {
                        throw new FormatException(string.Format("invalid size for value {0} in section {1}", index, info));
                    }

                    var buffer = input.ReadBytes(length);
                    section.PutData(index, buffer);
                }

                if (input.Position != end)
                {
                    throw new FormatException(string.Format("failed to read all of section {0}", info));
                }

                this.Sections.Add(section);
            }
        }

        public bool Contains(uint objectId, uint instanceId)
        {
            return this.Sections.Any(s => s.ObjectId == objectId &&
                                          s.InstanceId == instanceId);
        }

        public Save.Section this[uint objectId, uint instanceId]
        {
            get
            {
                return this.Sections.SingleOrDefault(s => s.ObjectId == objectId &&
                                                          s.InstanceId == instanceId);
            }

            set
            {
                this.Sections.RemoveAll(s => s.ObjectId == objectId &&
                                             s.InstanceId == instanceId);
                this.Sections.Add(value);
            }
        }

        private class SectionInfo
        {
            public uint ObjectId;
            public uint InstanceId;
            public uint Offset;
            public uint Size;

            public void Serialize(Stream output, Endian endian)
            {
                output.WriteValueU32(this.ObjectId, endian);
                output.WriteValueU32(this.InstanceId, endian);
                output.WriteValueU32(this.Offset, endian);
                output.WriteValueU32(this.Size, endian);
            }

            public void Deserialize(Stream input, Endian endian)
            {
                this.ObjectId = input.ReadValueU32(endian);
                this.InstanceId = input.ReadValueU32(endian);
                this.Offset = input.ReadValueU32(endian);
                this.Size = input.ReadValueU32(endian);
            }

            public override string ToString()
            {
                return string.Format("{0:X8}_{1:X8}", this.ObjectId, this.InstanceId);
            }
        }
    }
}
