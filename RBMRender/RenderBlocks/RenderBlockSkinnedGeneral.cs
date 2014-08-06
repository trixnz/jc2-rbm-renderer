using System.Collections.Generic;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockSkinnedGeneral : RenderBlockBase<SkinnedGeneral>
	{
		public RenderBlockSkinnedGeneral(GameWorld game, ArchiveWrapper archive, SkinnedGeneral block)
			: base(game, archive, block)
		{
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			if (Block.HasBigVertices)
			{
				for (int i = 0; i < Block.VertexData0Big.Count; i++)
				{
					SkinnedGeneralData0Big generalDataBig = Block.VertexData0Big[i];
					SkinnedGeneralData1 generalData = Block.VertexData1[i];

					var vertex = new VertexPositionNormalTextureTangent();
					vertex.Position = new Vector3(generalDataBig.PositionX, generalDataBig.PositionY, generalDataBig.PositionZ);
					vertex.Normal = Vector3.Zero;
					vertex.TextureCoordinate = new Vector2(generalData.U, generalData.V);
					vertex.Tangent = Vector3.Zero;

					vertices.Add(vertex);
				}
			}
			else
			{
				for (int i = 0; i < Block.VertexData0Small.Count; i++)
				{
					SkinnedGeneralData0Small generalDataSmall = Block.VertexData0Small[i];
					SkinnedGeneralData1 generalData = Block.VertexData1[i];

					var vertex = new VertexPositionNormalTextureTangent();
					vertex.Position = new Vector3(generalDataSmall.PositionX, generalDataSmall.PositionY, generalDataSmall.PositionZ);
					vertex.Normal = Vector3.Zero;
					vertex.TextureCoordinate = new Vector2(generalData.U, generalData.V);
					vertex.Tangent = Vector3.Zero;

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