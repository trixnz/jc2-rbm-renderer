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
    public class RawPropertyFile : IPropertyFile
    {
        public Endian Endian;

        private readonly List<Property.Node> _Nodes
            = new List<Property.Node>();

        public List<Property.Node> Nodes
        {
            get { return this._Nodes; }
        }

        #region SectionType
        private enum SectionType : byte
        {
            // ReSharper disable UnusedMember.Local
            Invalid = 0,
            // ReSharper restore UnusedMember.Local
            Node = 1,
            Variant = 2,
            Tag = 3,
            NodeByHash = 4,
            VariantByHash = 5,
        }
        #endregion

        #region VariantType
        // ReSharper disable InconsistentNaming
        private enum VariantType : byte
        {
            Integer = 1,
            Float = 2,
            String = 3,
            Vector2 = 4,
            Vector3 = 5,
            Vector4 = 6,
            Matrix3x3 = 7,
            Matrix4x3 = 8,
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

            if (type == typeof(Property.Matrix4x3Variant))
            {
                return VariantType.Matrix4x3;
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

        private void SerializeVariant(Stream output, Property.IVariant variant)
        {
            var endian = this.Endian;

            var type = GetVariantType(variant);
            output.WriteValueU8((byte)type);

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

                    string s = temp.Value;
                    if (s.Length > 0xFFFF)
                    {
                        throw new InvalidOperationException();
                    }

                    output.WriteValueU16((ushort)s.Length, endian);
                    output.WriteString(s, Encoding.ASCII);
                    return;
                }

                case VariantType.Vector2:
                {
                    var temp = (Property.Vector2Variant)variant;
                    output.WriteValueF32(temp.X, endian);
                    output.WriteValueF32(temp.Y, endian);
                    return;
                }

                case VariantType.Vector3:
                {
                    var temp = (Property.Vector3Variant)variant;
                    output.WriteValueF32(temp.X, endian);
                    output.WriteValueF32(temp.Y, endian);
                    output.WriteValueF32(temp.Z, endian);
                    return;
                }

                case VariantType.Vector4:
                {
                    var temp = (Property.Vector4Variant)variant;
                    output.WriteValueF32(temp.X, endian);
                    output.WriteValueF32(temp.Y, endian);
                    output.WriteValueF32(temp.Z, endian);
                    output.WriteValueF32(temp.W, endian);
                    return;
                }

                case VariantType.Matrix3x3:
                {
                    var temp = (Property.Matrix3x3Variant)variant;
                    output.WriteValueF32(temp.M11, endian);
                    output.WriteValueF32(temp.M12, endian);
                    output.WriteValueF32(temp.M13, endian);
                    output.WriteValueF32(temp.M21, endian);
                    output.WriteValueF32(temp.M22, endian);
                    output.WriteValueF32(temp.M23, endian);
                    output.WriteValueF32(temp.M31, endian);
                    output.WriteValueF32(temp.M32, endian);
                    output.WriteValueF32(temp.M33, endian);
                    return;
                }

                case VariantType.Matrix4x3:
                {
                    var temp = (Property.Matrix4x3Variant)variant;
                    output.WriteValueF32(temp.M11, endian);
                    output.WriteValueF32(temp.M12, endian);
                    output.WriteValueF32(temp.M13, endian);
                    output.WriteValueF32(temp.M21, endian);
                    output.WriteValueF32(temp.M22, endian);
                    output.WriteValueF32(temp.M23, endian);
                    output.WriteValueF32(temp.M31, endian);
                    output.WriteValueF32(temp.M32, endian);
                    output.WriteValueF32(temp.M33, endian);
                    output.WriteValueF32(temp.M41, endian);
                    output.WriteValueF32(temp.M42, endian);
                    output.WriteValueF32(temp.M43, endian);
                    return;
                }

                case VariantType.Integers:
                {
                    var temp = (Property.IntegersVariant)variant;
                    output.WriteValueS32(temp.Values.Count, endian);
                    foreach (int value in temp.Values)
                    {
                        output.WriteValueS32(value, endian);
                    }
                    return;
                }

                case VariantType.Floats:
                {
                    var temp = (Property.FloatsVariant)variant;
                    output.WriteValueS32(temp.Values.Count, endian);
                    foreach (int value in temp.Values)
                    {
                        output.WriteValueF32(value, endian);
                    }
                    return;
                }
            }

            throw new FormatException("unsupported variant type " + type.ToString());
        }

        private void SerializeNode(Stream output, Property.Node node)
        {
            var endian = this.Endian;

            byte count = 0;

            var nodesByName = node.Nodes
                .Where(kv => node.KnownNames.ContainsKey(kv.Key) == true)
                .ToArray();

            var variantsByName = node.Variants
                .Where(kv => node.KnownNames.ContainsKey(kv.Key) == true)
                .ToArray();

            var nodesByHash = node.Nodes
                .Where(kv => node.KnownNames.ContainsKey(kv.Key) == false)
                .ToArray();

            var variantsByHash = node.Variants
                .Where(kv => node.KnownNames.ContainsKey(kv.Key) == false)
                .ToArray();

            if (nodesByName.Length > 0)
            {
                count++;
            }

            if (variantsByName.Length > 0)
            {
                count++;
            }

            if (nodesByHash.Length > 0)
            {
                count++;
            }

            if (variantsByHash.Length > 0)
            {
                count++;
            }

            if (node.Tag != null)
            {
                count++;
            }

            output.WriteValueU8(count);

            if (nodesByName.Length > 0)
            {
                if (nodesByName.Length > 0xFFFF)
                {
                    throw new InvalidOperationException();
                }

                output.WriteValueU16((ushort)SectionType.Node, endian);
                output.WriteValueU16((ushort)nodesByName.Length, endian);
                foreach (var kv in nodesByName)
                {
                    var name = node.KnownNames[kv.Key];
                    output.WriteValueS32(name.Length, endian);
                    output.WriteString(name, Encoding.ASCII);
                    this.SerializeNode(output, kv.Value);
                }
            }

            if (variantsByName.Length > 0)
            {
                if (variantsByName.Length > 0xFFFF)
                {
                    throw new InvalidOperationException();
                }

                output.WriteValueU16((ushort)SectionType.Variant, endian);
                output.WriteValueU16((ushort)variantsByName.Length, endian);
                foreach (var kv in variantsByName)
                {
                    var name = node.KnownNames[kv.Key];
                    output.WriteValueS32(name.Length, endian);
                    output.WriteString(name, Encoding.ASCII);
                    this.SerializeVariant(output, kv.Value);
                }
            }

            if (node.Tag != null)
            {
                var bytes = Encoding.ASCII.GetBytes(node.Tag);

                if (bytes.Length > 0xFFFF)
                {
                    throw new InvalidOperationException();
                }

                output.WriteValueU16(3, endian);
                output.WriteValueU16((ushort)bytes.Length, endian);
                output.WriteBytes(bytes);
            }

            if (nodesByHash.Length > 0)
            {
                if (nodesByHash.Length > 0xFFFF)
                {
                    throw new InvalidOperationException();
                }

                output.WriteValueU16((ushort)SectionType.NodeByHash, endian);
                output.WriteValueU16((ushort)nodesByHash.Length, endian);
                foreach (var kv in nodesByHash)
                {
                    output.WriteValueU32(kv.Key, endian);
                    this.SerializeNode(output, kv.Value);
                }
            }

            if (variantsByHash.Length > 0)
            {
                if (variantsByHash.Length > 0xFFFF)
                {
                    throw new InvalidOperationException();
                }

                output.WriteValueU16((ushort)SectionType.VariantByHash, endian);
                output.WriteValueU16((ushort)variantsByHash.Length, endian);
                foreach (var kv in variantsByHash)
                {
                    output.WriteValueU32(kv.Key, endian);
                    this.SerializeVariant(output, kv.Value);
                }
            }
        }

        public void Serialize(Stream output)
        {
            foreach (var node in this.Nodes)
            {
                this.SerializeNode(output, node);
            }
        }

        private Property.IVariant DeserializeVariant(Stream input)
        {
            var endian = this.Endian;
            var type = (VariantType)input.ReadValueU8();

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
                    var variant = new Property.StringVariant();
                    ushort length = input.ReadValueU16(endian);
                    variant.Value = input.ReadString(length, true, Encoding.ASCII);
                    return variant;
                }

                case VariantType.Vector2:
                {
                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Vector2Variant();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    variant.X = input.ReadValueF32(endian);
                    variant.Y = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Vector3:
                {
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

                case VariantType.Matrix4x3:
                {
                    // ReSharper disable UseObjectOrCollectionInitializer
                    var variant = new Property.Matrix4x3Variant();
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
                    variant.M41 = input.ReadValueF32(endian);
                    variant.M42 = input.ReadValueF32(endian);
                    variant.M43 = input.ReadValueF32(endian);
                    return variant;
                }

                case VariantType.Integers:
                {
                    var variant = new Property.IntegersVariant();
                    int count = input.ReadValueS32(endian);
                    variant.Values.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        variant.Values.Add(input.ReadValueS32(endian));
                    }
                    return variant;
                }

                case VariantType.Floats:
                {
                    var variant = new Property.FloatsVariant();
                    int count = input.ReadValueS32(endian);
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

        private Property.Node DeserializeNode(Stream input)
        {
            var node = new Property.Node();
            var endian = this.Endian;

            var sectionsHandled = new List<SectionType>();
            var sectionCount = input.ReadValueU8();

            for (byte i = 0; i < sectionCount; i++)
            {
                var sectionType = (SectionType)input.ReadValueU16(endian);
                var elementCount = input.ReadValueU16(endian);

                if (sectionsHandled.Contains(sectionType) == true)
                {
                    throw new FormatException();
                }
                sectionsHandled.Add(sectionType);

                switch (sectionType)
                {
                    case SectionType.Node:
                    {
                        for (ushort j = 0; j < elementCount; j++)
                        {
                            var length = input.ReadValueU32(endian);
                            if (length >= 0x7FFF)
                            {
                                throw new FormatException();
                            }

                            var name = input.ReadString(length, true, Encoding.ASCII);
                            var id = name.HashJenkins();

                            if (node.KnownNames.ContainsKey(id) == false)
                            {
                                node.KnownNames.Add(id, name);
                            }
                            else if (node.KnownNames[id] != name)
                            {
                                throw new FormatException();
                            }

                            node.Nodes.Add(id, this.DeserializeNode(input));
                        }

                        break;
                    }

                    case SectionType.Variant:
                    {
                        for (ushort j = 0; j < elementCount; j++)
                        {
                            var length = input.ReadValueU32(endian);
                            if (length >= 0x7FFF)
                            {
                                throw new FormatException();
                            }

                            var name = input.ReadString(length, true, Encoding.ASCII);
                            var id = name.HashJenkins();

                            if (node.KnownNames.ContainsKey(id) == false)
                            {
                                node.KnownNames.Add(id, name);
                            }
                            else if (node.KnownNames[id] != name)
                            {
                                throw new FormatException();
                            }

                            node.Variants.Add(id, this.DeserializeVariant(input));
                        }

                        break;
                    }

                    case SectionType.Tag:
                    {
                        node.Tag = input.ReadString(elementCount, Encoding.ASCII);
                        break;
                    }

                    case SectionType.NodeByHash:
                    {
                        for (ushort j = 0; j < elementCount; j++)
                        {
                            var id = input.ReadValueU32(endian);
                            node.Nodes.Add(id, this.DeserializeNode(input));
                        }

                        break;
                    }

                    case SectionType.VariantByHash:
                    {
                        for (ushort j = 0; j < elementCount; j++)
                        {
                            var id = input.ReadValueU32(endian);
                            node.Variants.Add(id, this.DeserializeVariant(input));
                        }

                        break;
                    }

                    default:
                    {
                        throw new FormatException("unknown object section type " +
                                                  sectionType.ToString());
                    }
                }
            }

            return node;
        }

        public void Deserialize(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);

            this.Nodes.Clear();
            while (input.Position < input.Length)
            {
                this.Nodes.Add(this.DeserializeNode(input));
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

            var end = input.Position + length;

            this.Nodes.Clear();
            while (input.Position < end)
            {
                this.Nodes.Add(this.DeserializeNode(input));
            }

            if (input.Position != end)
            {
                throw new FormatException();
            }
        }
    }
}
