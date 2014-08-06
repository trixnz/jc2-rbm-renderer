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

namespace Gibbed.Avalanche.FileFormats.Save
{
    public class Section
    {
        public readonly uint ObjectId;
        public readonly uint InstanceId;
        internal readonly Dictionary<short, byte[]> Entries = new Dictionary<short, byte[]>();

        public Section(uint objectId, uint instanceId)
        {
            this.ObjectId = objectId;
            this.InstanceId = instanceId;
        }

        public void Clear()
        {
            this.Entries.Clear();
        }

        public bool Contains(short id)
        {
            return this.Contains(id, -1);
        }

        public bool Contains(short id, int size)
        {
            if (size >= 0)
            {
                return this.Entries.ContainsKey(id) &&
                       this.Entries[id].Length == size;
            }

            return this.Entries.ContainsKey(id);
        }

        private byte[] GetData(short id, int size)
        {
            if (this.Entries.ContainsKey(id) == false)
            {
                return null;
            }

            if (size >= 0 && this.Entries[id].Length != size)
            {
                return null;
            }

            return this.Entries[id];
        }

        public void PutData(short id, byte[] data)
        {
            this.Entries[id] = (byte[])data.Clone();
        }

        public bool? GetB8(short id)
        {
            var buffer = this.GetData(id, 1);
            if (buffer == null)
            {
                return null;
            }
            return buffer[0] != 0;
        }

        public void PutB8(short id, bool value)
        {
            this.PutData(id,
                         new[]
                         {
                             value == true ? (byte)1 : (byte)0
                         });
        }

        public byte? GetU8(short id)
        {
            var buffer = this.GetData(id, 1);
            if (buffer == null)
            {
                return null;
            }
            return buffer[0];
        }

        public void PutU8(short id, byte value)
        {
            this.PutData(id,
                         new[]
                         {
                             value
                         });
        }

        public int? GetS32(short id)
        {
            var buffer = this.GetData(id, 4);
            if (buffer == null)
            {
                return null;
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        public void PutS32(short id, int value)
        {
            this.PutData(id, BitConverter.GetBytes(value));
        }

        public uint? GetU32(short id)
        {
            var buffer = this.GetData(id, 4);
            if (buffer == null)
            {
                return null;
            }
            return BitConverter.ToUInt32(buffer, 0);
        }

        public void PutU32(short id, uint value)
        {
            this.PutData(id, BitConverter.GetBytes(value));
        }

        public float? GetF32(short id)
        {
            byte[] buffer = this.GetData(id, 4);
            if (buffer == null)
            {
                return null;
            }
            return BitConverter.ToSingle(buffer, 0);
        }

        public void PutF32(short id, float value)
        {
            this.PutData(id, BitConverter.GetBytes(value));
        }

        public KeyValuePair<uint, uint>? GetKeyValue(short index)
        {
            var buffer = this.GetData(index, 8);
            if (buffer == null)
            {
                return null;
            }

            var key = BitConverter.ToUInt32(buffer, 0);
            var value = BitConverter.ToUInt32(buffer, 4);
            return new KeyValuePair<uint, uint>(key, value);
        }

        public void PutKeyValue(short id, KeyValuePair<uint, uint> value)
        {
            var buffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(value.Key), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(value.Value), 0, buffer, 4, 4);
            this.PutData(id, buffer);
        }
    }
}
