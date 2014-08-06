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
    public class BlackboardPropertyFile : IPropertyFile
    {
        public Endian Endian;

        private readonly List<Property.Node> _Nodes
            = new List<Property.Node>();

        public List<Property.Node> Nodes
        {
            get { return this._Nodes; }
        }

        #region SectionType
        private enum SectionType : uint
        {
            // ReSharper disable UnusedMember.Local
            Invalid = 0,
            // ReSharper restore UnusedMember.Local
            Node = 1,
            Variant = 2,
        }
        #endregion

        #region VariantType
        // ReSharper disable InconsistentNaming
        private enum VariantType : uint
        {
            Integer = 1,
            Float = 2,
            String = 3,
            Vector2 = 4,
            Vector3 = 5,
            Vector4 = 6,
            Matrix3x3 = 7,
            Matrix4x4 = 8,
            Integers = 9,
            Floats = 10,
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region GetVariantType
        private static VariantType GetVariantType(Property.IVariant variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException("variant");
            }

            var type = variant.GetType();

            if (type == typeof(Property.IntegerVariant))
            {
                return VariantType.Integer;
            }

            if (type == typeof(Property.FloatVariant))
            {
                return VariantType.Float;
            }

            if (type == typeof(Property.StringVariant))
            {
                return VariantType.String;
            }

            if (type == typeof(Property.Vector2Variant))
            {
                return VariantType.Vector2;
            }

            if (type == typeof(Property.Vector3Variant))
            {
                return VariantType.Vector3;
            }

            if (type == typeof(Property.Vector4Variant))
            {
                return VariantType.Vector4;
            }

            if (type == typeof(Property.Matrix3x3Variant))
            {
                return VariantType.Matrix3x3;
            }

            if (type == typeof(Property.Matrix4x4Variant))
            {
                return VariantType.Matrix4x4;
            }

            if (type == typeof(Property.IntegersVariant))
            {
                return VariantType.Integers;
            }

            if (type == typeof(Property.FloatsVariant))
            {
                return VariantType.Floats;
            }

            throw new ArgumentException("unsupported variant type", "variant");
        }
        #endregion

        private void SerializeVariant(MemoryStream output, Property.IVariant variant)
        {
            var endian = this.Endian;
            var type = GetVariantType(variant);

            output.WriteValueEnum<VariantType>(type, endian);

            switch (type)
            {
                case VariantType.Integer:
                {
                    var temp = (Property.IntegerVariant)variant;
                    output.WriteValueS32(temp.Value, endian);
                    return;
                }

                case VariantType.Float:
                {
                    var temp = (Property.FloatVariant)variant;
                    output.WriteValueF32(temp.Value, endian);
                    return;
                }

                case VariantType.String:
                {
                    var temp = (Property.StringVariant)variant;

                    if (string.IsNullOrEmpty(temp.Value) == true)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteStringZ(temp.Value, Encoding.ASCII);
                    }

                    return;
                }

                case VariantType.Vector2:
                {
                    var temp = (Property.Vector2Variant)variant;

                    if (Equals(temp.X, 0.0f) == true &&
                        Equals(temp.Y, 0.0f) == true)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueF32(temp.X, endian);
                        output.WriteValueF32(temp.Y, endian);
                    }

                    return;
                }

                case VariantType.Vector3:
                {
                    var temp = (Property.Vector3Variant)variant;

                    if (Equals(temp.X, 0.0f) == true &&
                        Equals(temp.Y, 0.0f) == true &&
                        Equals(temp.Z, 0.0f) == true)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueF32(temp.X, endian);
                        output.WriteValueF32(temp.Y, endian);
                        output.WriteValueF32(temp.Z, endian);
                    }

                    return;
                }

                case VariantType.Vector4:
                {
                    var temp = (Property.Vector4Variant)variant;

                    if (Equals(temp.X, 0.0f) == true &&
                        Equals(temp.Y, 0.0f) == true &&
                        Equals(temp.Z, 0.0f) == true &&
                        Equals(temp.W, 0.0f) == true)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueF32(temp.X, endian);
                        output.WriteValueF32(temp.Y, endian);
                        output.WriteValueF32(temp.Z, endian);
                        output.WriteValueF32(temp.W, endian);
                    }

                    return;
                }

                case VariantType.Matrix3x3:
                {
                    var temp = (Property.Matrix3x3Variant)variant;

                    if (Equals(temp.M11, 0.0f) == true &&
                        Equals(temp.M12, 0.0f) == true &&
                        Equals(temp.M13, 0.0f) == true &&
                        Equals(temp.M21, 0.0f) == true &&
                        Equals(temp.M22, 0.0f) == true &&
                        Equals(temp.M23, 0.0f) == true &&
                        Equals(temp.M31, 0.0f) == true &&
                        Equals(temp.M32, 0.0f) == true &&
                        Equals(temp.M33, 0.0f) == true)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueF32(temp.M11, endian);
                        output.WriteValueF32(temp.M12, endian);
                        output.WriteValueF32(temp.M13, endian);
                        output.WriteValueF32(temp.M21, endian);
                        output.WriteValueF32(temp.M22, endian);
                        output.WriteValueF32(temp.M23, endian);
                        output.WriteValueF32(temp.M31, endian);
                        output.WriteValueF32(temp.M32, endian);
                        output.WriteValueF32(temp.M33, endian);
                    }

                    return;
                }

                case VariantType.Matrix4x4:
                {
                    var temp = (Property.Matrix4x4Variant)variant;

                    if (Equals(temp.M11, 0.0f) == true &&
                        Equals(temp.M12, 0.0f) == true &&
                        Equals(temp.M13, 0.0f) == true &&
                        Equals(temp.M14, 0.0f) == true &&
                        Equals(temp.M21, 0.0f) == true &&
                        Equals(temp.M22, 0.0f) == true &&
                        Equals(temp.M23, 0.0f) == true &&
                        Equals(temp.M24, 0.0f) == true &&
                        Equals(temp.M31, 0.0f) == true &&
                        Equals(temp.M32, 0.0f) == true &&
                        Equals(temp.M33, 0.0f) == true &&
                        Equals(temp.M34, 0.0f) == true &&
                        Equals(temp.M41, 0.0f) == true &&
                        Equals(temp.M42, 0.0f) == true &&
                        Equals(temp.M43, 0.0f) == true &&
                        Equals(temp.M44, 0.0f) == true)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueF32(temp.M11, endian);
                        output.WriteValueF32(temp.M12, endian);
                        output.WriteValueF32(temp.M13, endian);
                        output.WriteValueF32(temp.M14, endian);
                        output.WriteValueF32(temp.M21, endian);
                        output.WriteValueF32(temp.M22, endian);
                        output.WriteValueF32(temp.M23, endian);
                        output.WriteValueF32(temp.M24, endian);
                        output.WriteValueF32(temp.M31, endian);
                        output.WriteValueF32(temp.M32, endian);
                        output.WriteValueF32(temp.M33, endian);
                        output.WriteValueF32(temp.M34, endian);
                        output.WriteValueF32(temp.M41, endian);
                        output.WriteValueF32(temp.M42, endian);
                        output.WriteValueF32(temp.M43, endian);
                        output.WriteValueF32(temp.M44, endian);
                    }

                    return;
                }

                case VariantType.Integers:
                {
                    var temp = (Property.IntegersVariant)variant;

                    if (temp.Values.Count == 0)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueS32(temp.Values.Count * 4, endian);
                        foreach (var value in temp.Values)
                        {
                            output.WriteValueS32(value, endian);
                        }
                    }

                    return;
                }

                case VariantType.Floats:
                {
                    var temp = (Property.FloatsVariant)variant;

                    if (temp.Values.Count == 0)
                    {
                        output.WriteValueS32(-1, endian);
                    }
                    else
                    {
                        output.WriteValueU32((uint)output.Position + 4, endian);
                        output.WriteValueS32(temp.Values.Count * 4, endian);
                        foreach (var value in temp.Values)
                        {
                            output.WriteValueF32(value, endian);
                        }
                    }

                    return;
                }
            }

            throw new FormatException("unsupported variant type " + type.ToString());
        }

        private void SerializeNode(MemoryStream output, Property.Node node)
        {
            var endian = this.Endian;

            if (node.Tag != null)
            {
                throw new InvalidOperationException("node tags not supported");
            }

            var nodes = new List<KeyValuePair<long, Property.Node>>();
            foreach (var kv in node.Nodes)
            {
                var baseOffset = output.Position;
                output.WriteValueU32(kv.Key, endian);
                output.WriteValueEnum<SectionType>(SectionType.Node, endian);
                output.WriteValueS32(-1, endian);
                output.WriteValueU32((uint)(baseOffset + 16), endian);

                nodes.Add(new KeyValuePair<long, Property.Node>(baseOffset + 8, kv.Value));
            }

            var variants = new List<KeyValuePair<long, Property.IVariant>>();
            foreach (var kv in node.Variants)
            {
                var baseOffset = output.Position;
                output.WriteValueU32(kv.Key, endian);
                output.WriteValueEnum<SectionType>(SectionType.Variant, endian);
                output.WriteValueS32(-1, endian);
                output.WriteValueU32((uint)(baseOffset + 16), endian);

                variants.Add(new KeyValuePair<long, Property.IVariant>(baseOffset + 8, kv.Value));
            }

            output.Seek(-4, SeekOrigin.Current);
            output.WriteValueS32(-1, endian);

            var dataNodes = new List<KeyValuePair<long, Property.Node>>();
            foreach (var kv in nodes)
            {
                if (kv.Value.Nodes.Count == 0 &&
                    kv.Value.Variants.Count == 0)
                {
                    continue;
                }

                var baseOffset = output.Position;

                dataNodes.Add(new KeyValuePair<long, Property.Node>(output.Position, kv.Value));
                output.Seek(4, SeekOrigin.Current);

                var nextOffset = output.Position;

                output.Position = kv.Key;
                output.WriteValueU32((uint)baseOffset, endian);

                output.Position = nextOffset;
            }

            //output.Position = output.Position.Align(16);

            foreach (var kv in variants)
            {
                if (kv.Value != null)
                {
                    var baseOffset = output.Position;
                    this.SerializeVariant(output, kv.Value);

                    var nextOffset = output.Position;

                    output.Position = kv.Key;
                    output.WriteValueU32((uint)baseOffset, endian);

                    output.Position = nextOffset.Align(4);
                    //output.Position = nextOffset.Align(16);
                }
            }

            foreach (var kv in dataNodes)
            {
                var baseOffset = output.Position;
                this.SerializeNode(output, kv.Value);

                var nextOffset = output.Position;

                output.Position = kv.Key;
                output.WriteValueU32((uint)baseOffset, endian);

                output.Position = nextOffset;
            }
        }

        public void Serialize(Stream output)
        {
            var endian = this.Endian;

            foreach (var node in this.Nodes)
            {
                using (var memory = new MemoryStream())
                {
                    this.SerializeNode(memory, node);

                    memory.Position = 0;
                    output.WriteValueU32(0x50434242, Endian.Big);
                    output.WriteValueU32((uint)memory.Length, endian);
                    output.WriteFromStream(memory, memory.Length);
                }
            }
        }

        private Property.IVariant DeserializeVariant(MemoryStream input)
        {
            var endian = this.Endian;
            var type = input.ReadValueEnum<VariantType>();

            switch (type)
            {
                case VariantType.Integer:
                {
                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.IntegerVariant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.Value = input.ReadValueS32(endian);
                    return variant;
                }

                case VariantType.Float:
                {
                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.FloatVariant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.Value = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.String:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.StringVariant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.StringVariant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.Value = input.ReadStringZ(Encoding.ASCII);
                    return variant;
                }

                case VariantType.Vector2:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.Vector2Variant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Vector2Variant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.X = input.ReadValueF32(endian);
                    variant.Y = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Vector3:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.Vector3Variant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Vector3Variant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.X = input.ReadValueF32(endian);
                    variant.Y = input.ReadValueF32(endian);
                    variant.Z = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Vector4:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.Vector4Variant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Vector4Variant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.X = input.ReadValueF32(endian);
                    variant.Y = input.ReadValueF32(endian);
                    variant.Z = input.ReadValueF32(endian);
                    variant.W = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Matrix3x3:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.Matrix3x3Variant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Matrix3x3Variant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.M11 = input.ReadValueF32(endian);
                    variant.M12 = input.ReadValueF32(endian);
                    variant.M13 = input.ReadValueF32(endian);
                    variant.M21 = input.ReadValueF32(endian);
                    variant.M22 = input.ReadValueF32(endian);
                    variant.M23 = input.ReadValueF32(endian);
                    variant.M31 = input.ReadValueF32(endian);
                    variant.M32 = input.ReadValueF32(endian);
                    variant.M33 = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Matrix4x4:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.Matrix4x4Variant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Matrix4x4Variant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.M11 = input.ReadValueF32(endian);
                    variant.M12 = input.ReadValueF32(endian);
                    variant.M13 = input.ReadValueF32(endian);
                    variant.M14 = input.ReadValueF32(endian);
                    variant.M21 = input.ReadValueF32(endian);
                    variant.M22 = input.ReadValueF32(endian);
                    variant.M23 = input.ReadValueF32(endian);
                    variant.M24 = input.ReadValueF32(endian);
                    variant.M31 = input.ReadValueF32(endian);
                    variant.M32 = input.ReadValueF32(endian);
                    variant.M33 = input.ReadValueF32(endian);
                    variant.M34 = input.ReadValueF32(endian);
                    variant.M41 = input.ReadValueF32(endian);
                    variant.M42 = input.ReadValueF32(endian);
                    variant.M43 = input.ReadValueF32(endian);
                    variant.M44 = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Integers:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.IntegersVariant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    var size = input.ReadValueS32(endian);
                    if ((size % 4) != 0)
                    {
                        throw new FormatException();
                    }

                    var variant = new Property.IntegersVariant();
                    int count = size / 4;
                    variant.Values.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        variant.Values.Add(input.ReadValueS32(endian));
                    }
                    return variant;
                }

                case VariantType.Floats:
                {
                    var offset = input.ReadValueS32(endian);
                    if (offset == -1)
                    {
                        return new Property.FloatsVariant();
                    }
                    input.Seek(offset, SeekOrigin.Begin);

                    var size = input.ReadValueS32(endian);
                    if ((size % 4) != 0)
                    {
                        throw new FormatException();
                    }

                    var variant = new Property.FloatsVariant();
                    int count = size / 4;
                    variant.Values.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        variant.Values.Add(input.ReadValueF32(endian));
                    }
                    return variant;
                }
            }

            throw new FormatException("unsupported variant type " + type.ToString());
        }

        private Property.Node DeserializeNode(MemoryStream input)
        {
            var node = new Property.Node();
            var endian = this.Endian;

            while (true)
            {
                var id = input.ReadValueU32(endian);
                var type = input.ReadValueEnum<SectionType>(endian);
                var dataOffset = input.ReadValueS32(endian);
                var nextOffset = input.ReadValueS32(endian);

                switch (type)
                {
                    case SectionType.Node:
                    {
                        // why? WHY? :(
                        if (dataOffset != -1)
                        {
                            input.Seek(dataOffset, SeekOrigin.Begin);
                            dataOffset = input.ReadValueS32(endian);
                        }

                        if (dataOffset == -1)
                        {
                            node.Nodes.Add(id, new Property.Node());
                        }
                        else
                        {
                            input.Seek(dataOffset, SeekOrigin.Begin);
                            node.Nodes.Add(id, this.DeserializeNode(input));
                        }

                        break;
                    }

                    case SectionType.Variant:
                    {
                        if (dataOffset == -1)
                        {
                            node.Variants.Add(id, null);
                        }
                        else
                        {
                            input.Seek(dataOffset, SeekOrigin.Begin);
                            node.Variants.Add(id, this.DeserializeVariant(input));
                        }

                        break;
                    }

                    default:
                    {
                        throw new FormatException("unknown object section type " +
                                                  type.ToString());
                    }
                }

                if (nextOffset == -1)
                {
                    break;
                }

                input.Seek(nextOffset, SeekOrigin.Begin);
            }

            return node;
        }

        public void Deserialize(Stream input)
        {
            var endian = this.Endian;

            this.Nodes.Clear();
            while (input.Position < input.Length)
            {
                if (CheckMagic(input) == false)
                {
                    throw new FormatException();
                }

                uint length = input.ReadValueU32(endian);
                if (input.Position + length > input.Length)
                {
                    throw new EndOfStreamException("object size greater than input size");
                }

                using (var memory = input.ReadToMemoryStream(length))
                {
                    this.Nodes.Add(this.DeserializeNode(memory));
                }
            }

            if (input.Position != input.Length)
            {
                throw new FormatException();
            }
        }

        public void Deserialize(Stream input, int length)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (length < 0 ||
                input.Position + length > input.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            var endian = this.Endian;

            var end = input.Position + length;

            this.Nodes.Clear();
            while (input.Position < end)
            {
                if (CheckMagic(input) == false)
                {
                    throw new FormatException();
                }

                uint objectLength = input.ReadValueU32(endian);
                if (input.Position + objectLength > input.Length)
                {
                    throw new EndOfStreamException("object size greater than input size");
                }

                using (var memory = input.ReadToMemoryStream(objectLength))
                {
                    this.Nodes.Add(this.DeserializeNode(memory));
                }
            }

            if (input.Position != end)
            {
                throw new FormatException();
            }
        }

        private const uint Magic = 0x50434242;

        public static bool CheckMagic(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Big);
            return magic == Magic; // 'PCBB' (big-endian) "Property Container Blackboard"?
        }
    }
}
