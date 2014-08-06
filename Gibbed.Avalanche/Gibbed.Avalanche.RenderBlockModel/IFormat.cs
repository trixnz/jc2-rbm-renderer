using System;
using System.Collections.Generic;
using Gibbed.IO;
using System.IO;
using System.Linq;
using System.Text;

namespace Gibbed.Avalanche.RenderBlockModel
{
    internal interface IFormat
    {
        void Serialize(Stream output, Endian endian);
        void Deserialize(Stream input, Endian endian);
    }
}
