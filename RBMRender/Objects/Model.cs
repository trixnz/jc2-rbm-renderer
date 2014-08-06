using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Avalanche.FileFormats;
using Gibbed.Avalanche.RenderBlockModel;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using RBMRender.RenderBlocks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using RasterizerState = SharpDX.Toolkit.Graphics.RasterizerState;

namespace RBMRender.Objects
{
	public class Model : GameSystem
	{
		private readonly GameWorld _game;
		private readonly List<IRenderBlockDrawable> _renderBlocks = new List<IRenderBlockDrawable>();
		private Color _baseDiffuseColor = new Color(130, 49, 145, 255);
		private Buffer<short> _indexBuffer;
		private VertexInputLayout _inputLayout;

		private Buffer<VertexPositionNormalTextureTangent> _vertexBuffer;

		public Model(GameWorld game, ArchiveWrapper archive, ModelFile modelFile) : base(game)
		{
			_game = game;

			Archive = archive;
			ModelFile = modelFile;

			Enabled = Visible = true;
		}

		public SmallArchiveFile SmallArc { get; set; }
		public FileStream File { get; set; }
		public ArchiveWrapper Archive { get; set; }
		public ModelFile ModelFile { get; set; }

		private void AddRenderBlock(Type blockType, IRenderBlock renderBlock)
		{
			_renderBlocks.Add(
				(IRenderBlockDrawable)
					Activator.CreateInstance(blockType, new object[] {_game, Archive, renderBlock}));
		}

		protected override void LoadContent()
		{
			var vertices = new List<VertexPositionNormalTextureTangent>();
			var indices = new List<short>();

			var renderBlockMapping = new Dictionary<Type, Type>
			                         {
				                         {typeof (CarPaint), typeof (RenderBlockCarPaint)},
				                         {typeof (CarPaintSimple), typeof (RenderBlockCarPaintSimple)},
				                         {typeof (DeformableWindow), typeof (RenderBlockDeformableWindow)},
				                         {typeof (General), typeof (RenderBlockGeneral)},
				                         {typeof (SkinnedGeneral), typeof (RenderBlockSkinnedGeneral)}
			                         };

			foreach (IRenderBlock renderBlock in ModelFile.Blocks)
			{
				Type renderBlockType = null;
				if (renderBlockMapping.TryGetValue(renderBlock.GetType(), out renderBlockType))
					AddRenderBlock(renderBlockType, renderBlock);
				else
				{
					_game.Messages.Add("Failed to load block " + renderBlock.GetType().Name);
				}
			}

			foreach (IRenderBlockDrawable renderBlock in _renderBlocks)
				renderBlock.Load(vertices, indices);

			if (vertices.Count > 0)
			{
				_vertexBuffer = Buffer.Vertex.New(Game.GraphicsDevice, vertices.ToArray());
				_indexBuffer = Buffer.Index.New(Game.GraphicsDevice, indices.ToArray());
				_inputLayout = VertexInputLayout.FromBuffer(0, _vertexBuffer);
			}
			else
			{
				Enabled = false;
			}
			base.LoadContent();
		}

		public override void Draw(GameTime gameTime)
		{
			Game.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
			Game.GraphicsDevice.SetIndexBuffer(_indexBuffer, false);
			Game.GraphicsDevice.SetVertexInputLayout(_inputLayout);

			Game.GraphicsDevice.SetRasterizerState(RasterizerState.New(Game.GraphicsDevice,
				new RasterizerStateDescription
				{
					CullMode = CullMode.None,
					FillMode = FillMode.Solid,
					IsDepthClipEnabled = false,
					IsMultisampleEnabled = true
				}));

			int baseIndex = 0;
			int baseVertex = 0;
			foreach (IRenderBlockDrawable renderBlock in _renderBlocks)
			{
				_game.NormalMappingEffect.Parameters["IsWindow"].SetValue(false);

				EffectParameter diffuse = _game.NormalMappingEffect.Parameters["Diffuse"];
				diffuse.SetValue(_baseDiffuseColor.ToVector4());

				renderBlock.Draw(ref baseVertex, ref baseIndex);
			}

			base.Draw(gameTime);
		}
	}
}