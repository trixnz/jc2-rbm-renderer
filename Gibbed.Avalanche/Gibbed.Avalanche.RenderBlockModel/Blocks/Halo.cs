using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Avalanche.RenderBlockModel.Blocks
{
	public class Halo : IRenderBlock
	{
		public Material Material { get; set; }
		public readonly List<short> Faces = new List<short>();
		public void Serialize(Stream output, Endian endian)
		{
			
		}

		public void Deserialize(Stream input, Endian endian)
		{
			byte version = input.ReadValueU8();

			if (version == 0)
			{
				Material = new Material();
				Material.Deserialize(input, endian);

				uint numVertices = input.ReadValueU32(endian);
				input.ReadBytes(numVertices*20);

				input.ReadFaces(Faces, endian);
			}
		}
	}
}