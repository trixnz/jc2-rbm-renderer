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
using System.Globalization;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats
{
    public class AnarkFile
    {
        public class Block
        {
            public uint Id;
            public ulong Flags;
            public uint Unknown;
            public Dictionary<uint, object> Attributes;

            public void Deserialize(Stream input)
            {
                uint size = input.ReadValueU32();
                this.Id = input.ReadValueU32();
                this.Flags = input.ReadValueU64();
                this.Unknown = input.ReadValueU32();
                this.DeserializeAttributes(input);
            }

            private void DeserializeAttributes(Stream input)
            {
                this.Attributes = new Dictionary<uint, object>();
                uint count = input.ReadValueU32();
                for (int i = 1; i < count; i++)
                {
                    uint index = input.ReadValueU32();
                    object value;

                    uint type = input.ReadValueU32();
                    uint size = type < 0x80 ? 1 : input.ReadValueU32();

                    switch (type)
                    {
                        case 5:
                        {
                            value = input.ReadValueU32();
                            break;
                        }

                        case 6:
                        {
                            value = input.ReadValueS8();
                            break;
                        }

                        case 7:
                        {
                            value = input.ReadValueS16();
                            break;
                        }

                        case 8:
                        {
                            value = input.ReadValueS32();
                            break;
                        }

                        case 9:
                        {
                            value = input.ReadValueF32();
                            break;
                        }

                            //case 10: value = input.ReadValueU64(); break;

                        case 10:
                        {
                            value = input.ReadValueU32();
                            break;
                        }

                        case 14:
                        {
                            uint length = input.ReadValueU32();
                            if (length > 0x7FFF)
                            {
                                throw new FormatException();
                            }
                            value = input.ReadString(length, true, Encoding.ASCII);
                            break;
                        }

                        default:
                        {
                            throw new InvalidOperationException("unhandled attribute type " +
                                                                type.ToString(CultureInfo.InvariantCulture));
                        }
                    }

                    this.Attributes.Add(index, value);
                }
            }
        }

        public void Deserialize(Stream input)
        {
            // header
            if (input.ReadValueU8() != 0)
            {
                throw new Exception();
            }

            input.Seek(3, SeekOrigin.Current);
            input.Seek(4, SeekOrigin.Current);

            if (input.ReadString(8, true, Encoding.ASCII) != "AnarkBGF")
            {
                throw new FormatException("invalid header tag");
            }

            if (input.ReadValueU32() != 1)
            {
                throw new FormatException("invalid header version");
            }

            input.Seek(4, SeekOrigin.Current);
            uint idSize = input.ReadValueU8();
            input.Seek(7, SeekOrigin.Current);

            if (idSize != 4)
            {
                throw new FormatException("don't know how to handle this id size");
            }

            // blocks
            uint size = input.ReadValueU32();
            uint count = input.ReadValueU32();

            for (uint i = 0; i < count; i++)
            {
                var block = new Block();
                block.Deserialize(input);
            }
        }
    }
}
