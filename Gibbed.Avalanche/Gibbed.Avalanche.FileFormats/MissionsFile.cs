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
    public class MissionsFile
    {
        public enum MissionType : uint
        {
            KeyMission = 3,
            FactionMission = 4,
            FactionTask = 5,
            StrongholdTakeover = 6,
            FactionInterest1 = 8,
            FactionInterest2 = 9,
            FactionInterest3 = 10,
            Challenge = 18,
        }

        public class Mission
        {
            public string Name;
            public uint Unknown20;
            public byte Unknown24;
            public byte Unknown25;
            public byte Unknown26;
            public byte Unknown27;
            public byte Unknown28;
            public byte Unknown29;
            public byte Unknown2A;
            public uint Unknown2C;
            public MissionType Type;
            public uint Unknown34;
            public uint Unknown38;

            public void Deserialize(Stream input)
            {
                this.Name = input.ReadString(32, Encoding.ASCII).StripJunk();
                this.Unknown20 = input.ReadValueU32();
                this.Unknown24 = input.ReadValueU8();
                this.Unknown25 = input.ReadValueU8();
                this.Unknown26 = input.ReadValueU8();
                this.Unknown27 = input.ReadValueU8();
                this.Unknown28 = input.ReadValueU8();
                this.Unknown29 = input.ReadValueU8();
                this.Unknown2A = input.ReadValueU8();
                input.Seek(1, SeekOrigin.Current);
                this.Unknown2C = input.ReadValueU32();
                this.Type = input.ReadValueEnum<MissionType>();
                this.Unknown34 = input.ReadValueU32();
                this.Unknown38 = input.ReadValueU32();
            }

            public void Serialize(Stream output)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format("{0} - {1}",
                                     this.Name,
                                     this.Type);
            }
        }

        public List<Mission> Missions = new List<Mission>();

        public void Deserialize(Stream input)
        {
            this.Missions.Clear();
            while (input.Position + 0x3C <= input.Length)
            {
                var mission = new Mission();
                mission.Deserialize(input);
                this.Missions.Add(mission);
            }
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }
    }
}
