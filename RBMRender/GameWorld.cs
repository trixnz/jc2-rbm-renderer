using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gibbed.Avalanche.FileFormats;
using Gibbed.Avalanche.RenderBlockModel;
using RBMRender.Components;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Model = RBMRender.Objects.Model;

namespace RBMRender
{
	public class GameWorld : Game
	{
		private readonly List<Model> _loadedModels = new List<Model>();
		private readonly MainWindow _mainWindow;
		public SmallArchiveFile LoadedArchive;
		public FileStream LoadedArchiveStream;
		private GraphicsDeviceManager _graphicsDeviceManager;

		public GameWorld(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			_graphicsDeviceManager = new GraphicsDeviceManager(this)
			                         {
				                         SynchronizeWithVerticalRetrace = true,
				                         PreferMultiSampling = true,
			                         };

			GameSystems.Add(new EffectCompilerSystem(this));

			Messages = new List<string>();

			Content.RootDirectory = "Content";
		}

		public SpriteFont Font { get; set; }
		public SpriteBatch Batch { get; set; }

		public Effect NormalMappingEffect { get; set; }

		public Camera Camera { get; set; }

		public List<string> Messages { get; set; }
		public TextureFactory TextureFactory { get; set; }

		protected override void LoadContent()
		{
			TextureFactory = new TextureFactory(GraphicsDevice);
			TextureFactory.LoadFailed += textureName => Messages.Add(textureName + " failed to load");

			Font = Content.Load<SpriteFont>("Arial16.tkf");
			Batch = new SpriteBatch(GraphicsDevice);

			Camera = new Camera(this);
			GameSystems.Add(Camera);

			NormalMappingEffect = Content.Load<Effect>("tex2d");

			// Load up the ice cream truck (if present) as the default model
			string path = Path.Combine(Content.RootDirectory, "lave.v003_icecreamdlc.eez");
			if (File.Exists(path))
				OpenArchive(path);

			base.LoadContent();
		}

		public void OpenArchive(string path)
		{
			Messages.Clear();

			foreach (Model loadedModel in _loadedModels)
			{
				GameSystems.Remove(loadedModel);
			}

			LoadedArchive = new SmallArchiveFile();

			LoadedArchiveStream = File.OpenRead(path);
			LoadedArchive.Deserialize(LoadedArchiveStream);

			_mainWindow.lstModels.Items.Clear();
			foreach (SmallArchiveFile.Entry entry1 in LoadedArchive.Entries)
			{
				if (entry1.Name.EndsWith("rbm"))
					_mainWindow.lstModels.Items.Add(entry1.Name);
			}
			_mainWindow.lstModels.Items.SortDescriptions.Add(
				new SortDescription("", ListSortDirection.Ascending));
			_mainWindow.lstModels.SelectedIndex = 0;
		}

		protected override void Update(GameTime gameTime)
		{
			Matrix worldViewProj = Camera.World*Camera.View*Camera.Projection;

			NormalMappingEffect.Parameters["WorldViewProj"].SetValue(worldViewProj);
			NormalMappingEffect.Parameters["ViewVector"].SetValue(Camera.View.Forward);
			NormalMappingEffect.Parameters["World"].SetValue(Camera.World);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			base.Draw(gameTime);

			DrawCameraState();
		}

		private void DrawCameraState()
		{
			Batch.Begin();

			var pos = new Vector2(8, 8);
			
			const float fontScale = 0.75f;
			foreach (string error in Messages)
			{
				Batch.DrawString(Font, error, pos, Color.LawnGreen, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

				pos.Y += Font.LineSpacing*fontScale;
			}

			Batch.End();
		}

		public void LoadRbm(string s)
		{
			foreach (SmallArchiveFile.Entry entry in LoadedArchive.Entries)
			{
				if (entry.Name == s)
				{
					LoadedArchiveStream.Seek(entry.Offset, SeekOrigin.Begin);

					try
					{
						var model = new ModelFile();
						model.Deserialize(LoadedArchiveStream);

						IEnumerable<string> blocks = model.Blocks.Select(b => b.GetType().Name);
						Messages.Add(string.Format("Loaded {0}/{1}", Path.GetFileName(LoadedArchiveStream.Name), entry.Name));
						Messages.Add("Blocks:");
						Messages.AddRange(blocks);

						var loadedModel = new Model(this, new ArchiveWrapper(LoadedArchive, LoadedArchiveStream), model);
						GameSystems.Add(loadedModel);
						_loadedModels.Add(loadedModel);
					}
					catch (NotSupportedException ex)
					{
						Messages.Add("Failed to load " + s);
					}
				}
			}
		}

		public void UnloadAllRbm()
		{
			foreach (Model loadedModel in _loadedModels)
			{
				GameSystems.Remove(loadedModel);
			}

			_loadedModels.Clear();
			Messages.Clear();
		}
	}
}