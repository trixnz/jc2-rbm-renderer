using System.Collections.Generic;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockDeformableWindow : RenderBlockBase<DeformableWindow>
	{
		public RenderBlockDeformableWindow(GameWorld game, SmallArchiveWrapper smallArchive, DeformableWindow block)
			: base(game, smallArchive, block)
		{
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			foreach (DeformableWindowData0 vertexData in Block.VertexData0)
			{
				var vertex = new VertexPositionNormalTextureTangent();
				vertex.Position = new Vector3(vertexData.PositionX, vertexData.PositionY, vertexData.PositionZ);
				vertex.Normal = Vector3.Zero;
				vertex.TextureCoordinate = new Vector2(vertexData.U, vertexData.V);

				vertices.Add(vertex);
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
			baseVertex += Block.VertexData0.Count;
		}
	}
}