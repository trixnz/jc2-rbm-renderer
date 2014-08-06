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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Avalanche.FileFormats
{
    public class ArchiveTableFile
    {
        public struct Entry
        {
            public uint Offset;
            public uint Size;
        }

        public uint Alignment;
        private readonly List<uint> _Keys = new List<uint>();
        private readonly Hashtable _Entries = new Hashtable();

        public IEnumerable<uint> Keys
        {
            get { return this._Keys; }
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public bool Contains(uint key)
        {
            return this._Entries.ContainsKey(key);
        }

        public Entry Get(uint key)
        {
            if (this._Entries.ContainsKey(key) == false)
            {
                throw new KeyNotFoundException();
            }

            return (Entry)this._Entries[key];
        }

        public void Set(uint key, Entry entry)
        {
            this._Entries[key] = entry;
        }

        public Entry this[uint key]
        {
            get { return this.Get(key); }
            set { this.Set(key, value); }
        }

        public void Set(uint key, uint offset, uint size)
        {
            this.Set(key,
                     new Entry()
                     {
                         Offset = offset,
                         Size = size,
                     });
        }

        public void Remove(uint key)
        {
            if (this._Entries.ContainsKey(key) == false)
            {
                throw new KeyNotFoundException();
            }

            this._Entries.Remove(key);
        }

        public void Move(uint oldKey, uint newKey)
        {
            if (this._Entries.ContainsKey(oldKey) == false)
            {
                throw new KeyNotFoundException();
            }

            if (this._Entries.ContainsKey(newKey) == true)
            {
                throw new ArgumentException("table already contains the new key", "newKey");
            }

            this._Entries[newKey] = this._Entries[oldKey];
            this._Entries.Remove(oldKey);
        }

        public void Deserialize(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);

            this.Alignment = input.ReadValueU32();

            if (this.Alignment != 0x0800 && this.Alignment.Swap() != 0x0800)
            {
                throw new FormatException("strange alignment");
            }

            Endian endian;

            if (this.Alignment == 0x0800)
            {
                endian = Endian.Little;
            }
            else
            {
                this.Alignment = this.Alignment.Swap();
                endian = Endian.Big;
            }

            this._Entries.Clear();
            this._Keys.Clear();
            while (input.Position + 12 <= input.Length)
            {
                var entry = new Entry();
                uint name = input.ReadValueU32(endian);
                entry.Offset = input.ReadValueU32(endian);
                entry.Size = input.ReadValueU32(endian);
                this._Keys.Add(name);
                this._Entries.Add(name, entry);
            }
        }
    }
}
