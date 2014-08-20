using System.Collections.Generic;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockCarPaintSimple : RenderBlockBase<CarPaintSimple>
	{
		public RenderBlockCarPaintSimple(GameWorld game, SmallArchiveWrapper smallArchive, CarPaintSimple block)
			: base(game, smallArchive, block)
		{
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			foreach (CarPaintSimple.Vertex gameVertex in Block.Vertices)
			{
				var vertex = new VertexPositionNormalTextureTangent();
				vertex.Position = new Vector3(gameVertex.PositionX, gameVertex.PositionY, gameVertex.PositionZ);
				vertex.Normal = Vector3.Zero;
				vertex.TextureCoordinate = new Vector2(gameVertex.U, gameVertex.V);

				vertices.Add(vertex);
			}

			indices.AddRange(Block.Faces);
		}

		public override void Draw(ref int baseVertex, ref int baseIndex)
		{
			SetTexture("DiffuseTexture", Block.Textures[0]);
			SetTexture("PropertiesTexture", Block.Textures[1]);
			SetTexture("NormalsTexture", Block.Textures[2]);

			foreach (EffectPass pass in Game.NormalMappingEffect.Techniques["CarPaint"].Passes)
			{
				pass.Apply();

				Game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, Block.Faces.Count, baseIndex, baseVertex);
			}

			baseIndex += Block.Faces.Count;
			baseVertex += Block.Vertices.Count;
		}
	}
}