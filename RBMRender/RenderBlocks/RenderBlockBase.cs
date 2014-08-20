using System.Collections.Generic;
using Gibbed.Avalanche.FileFormats;
using Gibbed.Avalanche.RenderBlockModel;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public abstract class RenderBlockBase<TBlock> : IRenderBlockDrawable where TBlock : IRenderBlock
	{
		protected RenderBlockBase(GameWorld game, SmallArchiveWrapper smallArchive, TBlock block)
		{
			Game = game;
			SmallArchive = smallArchive;
			Block = block;
		}

		protected GameWorld Game { get; set; }
		public SmallArchiveWrapper SmallArchive { get; set; }
		public TBlock Block { get; set; }

		public string Name
		{
			get { return typeof (TBlock).Name; }
		}

		public abstract void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices);
		public abstract void Draw(ref int baseVertex, ref int baseIndex);
		public int VertexCount { get; set; }

		private Texture2D GetTextureFromArchive(string textureName)
		{
			SmallArchiveFile.Entry entry = SmallArchive.SmallArchive.Entries.Find(e => e.Name == textureName);

			if (entry == null)
				return null;

			return Game.TextureFactory.LoadTexture(SmallArchive, entry);
		}

		protected void SetTexture(string effectVariable, string textureName)
		{
			Texture2D texture = GetTextureFromArchive(textureName) ?? Game.TextureFactory.LoadTexture(textureName);

			//if (texture == null)
//				return;

			EffectParameter parameter = Game.NormalMappingEffect.Parameters[effectVariable];

			if (parameter != null)
				parameter.SetResource(texture);
		}

		protected void DrawNormals(int faces, int baseIndex, int baseVertex)
		{
			Game.NormalsEffect.Parameters["NormalLength"].SetValue(GlobalSettings.Instance.NormalLength);

			foreach (EffectPass pass in Game.NormalsEffect.CurrentTechnique.Passes)
			{
				pass.Apply();

				Game.GraphicsDevice.DrawIndexed(PrimitiveType.PointList, faces, baseIndex, baseVertex);
			}
		}
	}
}