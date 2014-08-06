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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats
{
    /* Avalanche cooked up what appears to be a "generic" file format
     * and uses it for their shader bundles. The header of the file is
     * "ADF " (when swapped). I'm guessing this means
     * "Avalanche Data Format".
     */
    public class DataFormatFile
    {
        public Endian Endian = Endian.Little;
        public uint Version;

        public readonly List<DataFormat.Definition> Definitions =
            new List<DataFormat.Definition>();

        public readonly List<DataFormat.Entry> Entries =
            new List<DataFormat.Entry>();

        public void Deserialize(Stream input)
        {
            uint magic = input.ReadValueU32();
            if (magic != 0x41444620 &&
                magic.Swap() != 0x41444620)
            {
                throw new FormatException("not a ADF file");
            }

            var endian = magic == 0x41444620 // ' FDA'
                             ? Endian.Little
                             : Endian.Big;
            this.Version = input.ReadValueU32(endian);

            if (this.Version != 1)
            {
                throw new FormatException("unhandled ADF file version");
            }

            uint dataCount = input.ReadValueU32(endian);
            uint dataOffset = input.ReadValueU32(endian);
            uint definitionCount = input.ReadValueU32(endian);
            uint definitionOffset = input.ReadValueU32(endian);

            this.Definitions.Clear();
            if (definitionCount > 0)
            {
                input.Seek(definitionOffset, SeekOrigin.Begin);
                for (uint i = 0; i < definitionCount; i++)
                {
                    var definition = new DataFormat.Definition();
                    definition.Deserialize(input, endian);
                    this.Definitions.Add(definition);
                }
            }

            this.Entries.Clear();
            if (dataCount > 0)
            {
                input.Seek(dataOffset, SeekOrigin.Begin);
                for (uint i = 0; i < dataCount; i++)
                {
                    var entry = new DataFormat.Entry();
                    entry.Deserialize(input, endian);
                    this.Entries.Add(entry);
                }
            }

            this.Endian = endian;
        }

        public DataFormat.Definition FindDefinition(uint typeHash)
        {
            return this.Definitions
                .FirstOrDefault(e => e.TypeHash == typeHash);
        }

        public DataFormat.Entry FindEntry(uint typeHash)
        {
            return this.Entries
                .FirstOrDefault(e => e.TypeHash == typeHash);
        }

        private object ParseNativeValue(UInt32 typeHash, UInt32 size, Stream input)
        {
            var type = (DataFormat.NativeValueType)typeHash;

            switch (type)
            {
                case DataFormat.NativeValueType.UInt32:
                {
                    if (size != 4)
                    {
                        throw new InvalidOperationException("Native UInt32 size must be 4");
                    }

                    return input.ReadValueU32(this.Endian);
                }

                case DataFormat.NativeValueType.String:
                {
                    if (size != 4)
                    {
                        throw new InvalidOperationException("Native String type size must be 4");
                    }

                    uint offset = input.ReadValueU32(this.Endian);
                    input.Seek(offset, SeekOrigin.Begin);
                    return input.ReadStringZ(Encoding.ASCII);
                }

                default:
                {
                    throw new NotSupportedException("unhandled type " + type.ToString());
                }
            }
        }

        private object ParseNativeValue(long baseOffset, DataFormat.DefinitionValue definition, Stream input)
        {
            input.Seek(baseOffset + definition.Offset, SeekOrigin.Begin);
            return this.ParseNativeValue(definition.TypeHash, definition.Size, input);
        }

        private object ParseNativeArray(long baseOffset, uint count, uint typeHash, Stream input)
        {
            var type = (DataFormat.NativeValueType)typeHash;

            // handle byte arrays in a special way :)
            if (type == DataFormat.NativeValueType.UInt8)
            {
                input.Seek(baseOffset, SeekOrigin.Begin);
                var array = new byte[count];
                input.Read(array, 0, array.Length);
                return array;
            }

            throw new NotSupportedException("arrays not implemented for native types");
        }

        private static bool IsNativeType(UInt32 typeHash)
        {
            return Enum.IsDefined(
                typeof(DataFormat.NativeValueType),
                typeHash) == true;
        }

        private static bool IsNativeType(DataFormat.DefinitionValue definition)
        {
            return Enum.IsDefined(
                typeof(DataFormat.NativeValueType),
                definition.TypeHash) == true;
        }

        public object ParseDefinition(long baseOffset, DataFormat.Definition definition, Stream input)
        {
            if (definition.DefinitionType == DataFormat.DefinitionType.Structure)
            {
                var structure = new DataFormat.Structure();

                foreach (var valueDefinition in definition.ValueDefinitions)
                {
                    object value;

                    if (IsNativeType(valueDefinition) == true)
                    {
                        value = this.ParseNativeValue(baseOffset, valueDefinition, input);
                    }
                    else
                    {
                        var subDefinition = this.FindDefinition(valueDefinition.TypeHash);
                        if (subDefinition == null)
                        {
                            throw new InvalidOperationException("missing definition " +
                                                                valueDefinition.TypeHash.ToString("X8"));
                        }

                        value = this.ParseDefinition(
                            baseOffset + valueDefinition.Offset,
                            subDefinition,
                            input);
                    }

                    structure.Values.Add(valueDefinition.Name, value);
                }

                return structure;
            }

            if (definition.DefinitionType == DataFormat.DefinitionType.Array)
            {
                input.Seek(baseOffset, SeekOrigin.Begin);
                uint offset = input.ReadValueU32(this.Endian);
                uint count = input.ReadValueU32(this.Endian);
                object value;

                if (IsNativeType(definition.ElementTypeHash) == true)
                {
                    value = this.ParseNativeArray(
                        offset,
                        count,
                        definition.ElementTypeHash,
                        input);
                }
                else
                {
                    var elementDefinition = this.FindDefinition(definition.ElementTypeHash);

                    var elements = new List<object>();
                    for (uint i = 0; i < count; i++)
                    {
                        var element = this.ParseDefinition(
                            offset + (i * elementDefinition.Size),
                            elementDefinition,
                            input);
                        elements.Add(element);
                    }

                    value = elements;
                }

                return value;
            }

            throw new InvalidOperationException("unhandled " + definition.DefinitionType.ToString());
        }

        public object ParseEntry(DataFormat.Entry entry, Stream input)
        {
            var definition = this.FindDefinition(entry.TypeHash);
            if (definition == null)
            {
                throw new InvalidOperationException("missing definition " + entry.TypeHash.ToString("X8"));
            }

            input.Seek(entry.Offset, SeekOrigin.Begin);
            using (MemoryStream data = input.ReadToMemoryStream(entry.Size))
            {
                return this.ParseDefinition(0, definition, data);
            }
        }
    }
}
