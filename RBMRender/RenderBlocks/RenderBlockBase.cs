using System.Collections.Generic;
using Gibbed.Avalanche.FileFormats;
using Gibbed.Avalanche.RenderBlockModel;
using SharpDX.Toolkit.Graphics;

namespace RBMRender.RenderBlocks
{
	public abstract class RenderBlockBase<TBlock> : IRenderBlockDrawable where TBlock : IRenderBlock
	{
		protected RenderBlockBase(GameWorld game, ArchiveWrapper archive, TBlock block)
		{
			Game = game;
			Archive = archive;
			Block = block;
		}

		protected GameWorld Game { get; set; }
		public ArchiveWrapper Archive { get; set; }
		protected TBlock Block { get; set; }

		public abstract void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices);
		public abstract void Draw(ref int baseVertex, ref int baseIndex);

		private Texture2D GetTextureFromArchive(string textureName)
		{
			SmallArchiveFile.Entry entry = Archive.SmallArchive.Entries.Find(e => e.Name == textureName);

			if (entry == null)
				return null;

			return Game.TextureFactory.LoadTexture(Archive, entry);
		}

		protected void SetTexture(string effectVariable, string textureName)
		{
			Texture2D texture = GetTextureFromArchive(textureName) ?? Game.TextureFactory.LoadTexture(textureName);

			if (texture == null)
				return;

			EffectParameter parameter = Game.NormalMappingEffect.Parameters[effectVariable];

			if (parameter != null)
				parameter.SetResource(texture);
		}
	}
}