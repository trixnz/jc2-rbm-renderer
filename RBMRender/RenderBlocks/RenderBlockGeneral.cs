using System.Collections.Generic;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using SamplerState = SharpDX.Toolkit.Graphics.SamplerState;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockGeneral : RenderBlockBase<General>
	{
		public RenderBlockGeneral(GameWorld game, SmallArchiveWrapper smallArchive, General block)
			: base(game, smallArchive, block)
		{
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			if (Block.HasBigVertices)
			{
				foreach (GeneralData0Big bigVertice in Block.VertexData0Big)
				{
					var position = new Vector3(bigVertice.PositionX, bigVertice.PositionY, bigVertice.PositionZ);
					Vector3 normal = Vector3.Zero;
					var uv = new Vector2(0, 0);

					vertices.Add(new VertexPositionNormalTextureTangent(position, normal, uv));
				}
			}
			else
			{
				foreach (GeneralData0Small smallVertice in Block.VertexData0Small)
				{
					var vertex = new VertexPositionNormalTextureTangent();
					vertex.Position = new Vector3(smallVertice.PositionX, smallVertice.PositionY, smallVertice.PositionZ)*
					                  Block.Unknown12;
					vertex.Normal = new Vector3(smallVertice.Normal.X, smallVertice.Normal.Y, smallVertice.Normal.Z);
					vertex.TextureCoordinate = new Vector2(smallVertice.U, smallVertice.V)*
					                           new Vector2(Block.Unknown13, Block.Unknown14);
					vertex.TextureCoordinate2 = new Vector2(smallVertice.U2, smallVertice.V2)*
					                            new Vector2(Block.Unknown15, Block.Unknown16);
					// Just a guess at this stage
					vertex.Tangent = new Vector3(smallVertice.UnkPacked2.X, smallVertice.UnkPacked2.Y, smallVertice.UnkPacked2.Z);
					vertex.VertexColor = new Vector3(smallVertice.UnkPacked3.X, smallVertice.UnkPacked3.Y, smallVertice.UnkPacked3.Z);

					vertex.SpecularPower = new Vector2(Block.Unknown10, 0);

					if ((Block.Unknown19 & 0x20) == 0x20)
						vertex.SpecularPower.Y = 1;

					vertex.ChannelMask = new Vector4(Block.Unknown01, Block.Unknown02, Block.Unknown03, Block.Unknown04);

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
			SetTexture("DeformedDiffuseTexture", Block.Material.DeformedDiffuseTexture);

			EffectParameter world = Game.NormalMappingEffect.Parameters["World"];
			Matrix transform = Matrix.Translation(0, 0, 0);
			//world.SetValue(Game.Camera.World*transform);

			foreach (EffectPass pass in Game.NormalMappingEffect.Techniques["General"].Passes)
			{
				pass.Apply();

				Game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, Block.Faces.Count, baseIndex, baseVertex);
			}

			if (GlobalSettings.Instance.NormalDebugging)
				base.DrawNormals(Block.Faces.Count, baseIndex, baseVertex);

			baseIndex += Block.Faces.Count;
			if (Block.HasBigVertices)
				baseVertex += Block.VertexData0Big.Count;
			else
				baseVertex += Block.VertexData0Small.Count;
		}
	}
}