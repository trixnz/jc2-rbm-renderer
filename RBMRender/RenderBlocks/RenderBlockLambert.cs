using System.Collections.Generic;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockLambert : RenderBlockBase<Lambert>
	{
		public RenderBlockLambert(GameWorld game, ArchiveWrapper archive, Lambert block)
			: base(game, archive, block)
		{
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			if (Block.HasBigVertices)
			{
				foreach (LambertData0Big bigVertice in Block.VertexData0Big)
				{
					var position = new Vector3(bigVertice.PositionX, bigVertice.PositionY, bigVertice.PositionZ);
					Vector3 normal = Vector3.Zero;
					var uv = new Vector2(0, 0);

					vertices.Add(new VertexPositionNormalTextureTangent(position, normal, uv));
				}
			}
			else
			{
				foreach (LambertData0Small smallVertice in Block.VertexData0Small)
				{
					var vertex = new VertexPositionNormalTextureTangent();
					vertex.Position = new Vector3(smallVertice.PositionX, smallVertice.PositionY, smallVertice.PositionZ);
					vertex.TextureCoordinate = new Vector2(smallVertice.U, smallVertice.V) * new Vector2(1,1);

					vertices.Add(vertex);
				}
			}

			indices.AddRange(Block.Faces);
		}

		public override void Draw(ref int baseVertex, ref int baseIndex)
		{
			SetTexture("DiffuseTexture", Block.Material.UndeformedDiffuseTexture);
			SetTexture("PropertiesTexture", Block.Material.UndeformedPropertiesMap);
			SetTexture("NormalsTexture", Block.Material.UndeformedNormalMap);

			foreach (EffectPass pass in Game.NormalMappingEffect.CurrentTechnique.Passes)
			{
				pass.Apply();

				Game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, Block.Faces.Count, baseIndex, baseVertex);
			}

			baseIndex += Block.Faces.Count;
			if (Block.HasBigVertices)
				baseVertex += Block.VertexData0Big.Count;
			else
				baseVertex += Block.VertexData0Small.Count;
		}
	}
}