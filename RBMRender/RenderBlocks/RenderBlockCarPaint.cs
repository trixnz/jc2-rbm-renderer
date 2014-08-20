using System.Collections.Generic;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockCarPaint : RenderBlockBase<CarPaint>
	{
		public RenderBlockCarPaint(GameWorld game, SmallArchiveWrapper smallArchive, CarPaint block) : base(game, smallArchive, block)
		{
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			for (int i = 0; i < Block.VertexData0.Count; i++)
			{
				CarPaintData0 carPaintData0 = Block.VertexData0[i];
				CarPaintData1 carPaintData1 = Block.VertexData1[i];

				var vertex = new VertexPositionNormalTextureTangent();
				vertex.Position = new Vector3(carPaintData0.PositionX, carPaintData0.PositionY, carPaintData0.PositionZ);
				vertex.Normal = new Vector3(carPaintData1.Normal.X, carPaintData1.Normal.Y, carPaintData1.Normal.Z);
				vertex.Tangent = new Vector3(carPaintData1.Tangent.X, carPaintData1.Tangent.Y, carPaintData1.Tangent.Z);
				vertex.TextureCoordinate = new Vector2(carPaintData1.U, carPaintData1.V);

				vertices.Add(vertex);
			}

			indices.AddRange(Block.Faces);
		}

		public override void Draw(ref int baseVertex, ref int baseIndex)
		{
			SetTexture("DiffuseTexture", Block.Material.UndeformedDiffuseTexture);
			SetTexture("PropertiesTexture", Block.Material.UndeformedPropertiesMap);
			SetTexture("NormalsTexture", Block.Material.UndeformedNormalMap);

			EffectParameter Diffuse = Game.NormalMappingEffect.Parameters["Diffuse"];
			Diffuse.SetValue(new Vector4(Block.Unknown1.ColorTone1R, Block.Unknown1.ColorTone1G,
				Block.Unknown1.ColorTone1B, 1));

			foreach (EffectPass pass in Game.NormalMappingEffect.Techniques["CarPaint"].Passes)
			{
				pass.Apply();

				Game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, Block.Faces.Count, baseIndex, baseVertex);
			}

			if (GlobalSettings.Instance.NormalDebugging)
				base.DrawNormals(Block.Faces.Count, baseIndex, baseVertex);

			baseIndex += Block.Faces.Count;
			baseVertex += Block.VertexData0.Count;
		}
	}
}