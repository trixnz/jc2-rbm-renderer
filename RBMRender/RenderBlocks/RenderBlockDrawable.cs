using System.Collections.Generic;

namespace RBMRender.RenderBlocks
{
	public interface IRenderBlockDrawable
	{
		void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices);
		void Draw(ref int baseVertex, ref int baseIndex);
	}
}