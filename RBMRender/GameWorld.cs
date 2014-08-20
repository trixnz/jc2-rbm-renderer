using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gibbed.Avalanche.FileFormats;
using Gibbed.Avalanche.RenderBlockModel;
using RBMRender.Components;
using RBMRender.PostProcessing;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Diagnostics;
using SharpDX.Toolkit.Graphics;
using Model = RBMRender.Objects.Model;
using RasterizerState = SharpDX.Toolkit.Graphics.RasterizerState;
using SamplerState = SharpDX.Toolkit.Graphics.SamplerState;

namespace RBMRender
{
	public class GameWorld : Game
	{
		private readonly GraphicsDeviceManager _graphicsDeviceManager;
		private readonly MainWindow _mainWindow;
		private readonly object _shaderErrorLock = new object();
		private readonly List<string> _shaderErrors = new List<string>();
		public SmallArchiveWrapper LoadedArchive;

		private RenderTarget2D _baseRenderTarget;
		private DepthStencilBuffer _depthStencil;
		private FXAA _fxAA;
		private Model _loadedModel;
		private SamplerState _samplerState;

		public GameWorld(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			_graphicsDeviceManager = new GraphicsDeviceManager(this)
			                         {
				                         SynchronizeWithVerticalRetrace = true,
				                         PreferMultiSampling = true,
			                         };

			var effectCompiler = new EffectCompilerSystem(this);
			effectCompiler.CompilationStarted += EffectCompilerOnCompilationStarted;
			effectCompiler.CompilationError += EffectCompilerOnCompilationError;
			effectCompiler.CompilationEnded += EffectCompilerOnCompilationEnded;

			GameSystems.Add(effectCompiler);
			Messages = new List<string>();

			Content.RootDirectory = "Content";
		}

		public Model LoadedModel
		{
			get { return _loadedModel; }
			set
			{
				_loadedModel = value;
				OnPropertyChanged("LoadedModel");
			}
		}

		public SpriteFont Font { get; set; }
		public SpriteBatch Batch { get; set; }

		public Effect NormalMappingEffect { get; set; }

		public Camera Camera { get; set; }

		public List<string> Messages { get; set; }
		public TextureFactory TextureFactory { get; set; }

		public Effect NormalsEffect { get; set; }

		private void EffectCompilerOnCompilationStarted(object sender, EffectCompilerEventArgs effectCompilerEventArgs)
		{
			// SharpDX is a little eager on its file access and attempts to read a locked
			// file in an unchecked manner, resulting in an uncaught exception if encountered.
			// Slow it down just a tad.
			Thread.Sleep(50);
		}

		private void EffectCompilerOnCompilationError(object sender, EffectCompilerEventArgs effectCompilerEventArgs)
		{
			lock (_shaderErrorLock)
			{
				foreach (LogMessage m in effectCompilerEventArgs.Messages)
				{
					foreach (string message in m.Text.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries))
					{
						int separatorPos = message.IndexOf("): ");
						int lineSeparatorPos = message.Substring(0, separatorPos).LastIndexOf('(');
						string newMessage = Path.GetFileName(message.Substring(0, lineSeparatorPos)) + message.Substring(lineSeparatorPos);
						if (!_shaderErrors.Contains(newMessage))
						{
							_shaderErrors.Add(newMessage);
						}
					}
				}
			}
		}

		private void EffectCompilerOnCompilationEnded(object sender, EffectCompilerEventArgs effectCompilerEventArgs)
		{
			lock (_shaderErrorLock)
				_shaderErrors.Clear();
		}

		protected override void LoadContent()
		{
			_graphicsDeviceManager.DeviceChangeEnd += (s, e) => CreateRenderTargets();
			CreateRenderTargets();

			_fxAA = new FXAA(this);
			_fxAA.LoadContent();

			TextureFactory = new TextureFactory(GraphicsDevice);
			TextureFactory.LoadFailed += textureName => Messages.Add(textureName + " failed to load");

			Font = Content.Load<SpriteFont>("Arial16.tkf");
			Batch = new SpriteBatch(GraphicsDevice);

			Camera = new Camera(this);
			GameSystems.Add(Camera);

			NormalMappingEffect = Content.Load<Effect>("tex2d");
			NormalsEffect = Content.Load<Effect>("normals");

			SamplerStateDescription samplerStateDesc = SamplerStateDescription.Default();
			samplerStateDesc.AddressU = TextureAddressMode.Wrap;
			samplerStateDesc.AddressV = TextureAddressMode.Wrap;
			samplerStateDesc.AddressW = TextureAddressMode.Wrap;
			samplerStateDesc.Filter = Filter.Anisotropic;

			_samplerState = SamplerState.New(GraphicsDevice, samplerStateDesc);

			// Load up the ice cream truck (if present) as the default model
			//string path = Path.Combine(Content.RootDirectory, "lave.v003_icecreamdlc.eez");
			//if (File.Exists(path))
				//OpenArchive(path);

			base.LoadContent();
		}

		private void CreateRenderTargets()
		{
			_baseRenderTarget = RenderTarget2D.New(GraphicsDevice, GraphicsDevice.BackBuffer.Width,
				GraphicsDevice.BackBuffer.Height, 1, PixelFormat.R32G32B32A32.Float);

			_depthStencil = DepthStencilBuffer.New(GraphicsDevice, GraphicsDevice.BackBuffer.Width,
				GraphicsDevice.BackBuffer.Height, DepthFormat.Depth32);
		}

		public void OpenFromArchive(SmallArchiveWrapper archive, string file)
		{
			UnloadAllRbm();

			LoadedArchive = archive;
			LoadRbm(file);

			CenterCameraOnScene();
		}

		public void OpenArchive(string path)
		{
			var smallArc = new SmallArchiveFile();
			LoadedArchive = new SmallArchiveWrapper(
				smallArc,
				File.OpenRead(path));
			smallArc.Deserialize(LoadedArchive.Reader);

			OpenFromArchive(LoadedArchive, "mc01_lod1-rico.rbm");
			CenterCameraOnScene();
		}


		protected override void Update(GameTime gameTime)
		{
			Matrix worldViewProj = Camera.View*Camera.Projection;

			EffectParameter eff = NormalsEffect.Parameters["WorldViewProj"];
			if (eff != null)
				eff.SetValue(worldViewProj);
			NormalMappingEffect.Parameters["WorldViewProj"].SetValue(worldViewProj);
			NormalMappingEffect.Parameters["ViewVector"].SetValue(Camera.View.Forward);
			NormalMappingEffect.Parameters["CameraPosition"].SetValue(Camera.Position);

			EffectParameter sampler = NormalMappingEffect.Parameters["WrappingSampler"];
			if (sampler != null)
				sampler.SetResource(_samplerState);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			DepthStencilView oldDepth;
			RenderTargetView[] gameTargets = GraphicsDevice.GetRenderTargets(out oldDepth);

			GraphicsDevice.Clear(Color.Black);

			GraphicsDevice.SetRenderTargets(GraphicsDevice.DepthStencilBuffer, _baseRenderTarget);
			GraphicsDevice.Clear(new Color4(0.2f, 0.2f, 0.2f, 1f));

			base.Draw(gameTime);

			GraphicsDevice.SetRenderTargets(oldDepth, gameTargets);
			_fxAA.DoPostProcessing(_baseRenderTarget);
			DrawCameraState();
		}

		private void DrawCameraState()
		{
			Batch.Begin();

			var pos = new Vector2(8, 8);

			List<string> messages = Messages.ToList();

			lock (_shaderErrorLock)
				_shaderErrors.ForEach(messages.Add);

			const float fontScale = 0.75f;
			foreach (string error in messages)
			{
				Batch.DrawString(Font, error, pos, Color.LawnGreen, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

				pos.Y += Font.LineSpacing*fontScale;
			}

			Batch.End();
		}

		public void LoadRbm(string s)
		{
			SmallArchiveFile smallArc = LoadedArchive.SmallArchive;

			foreach (SmallArchiveFile.Entry entry in smallArc.Entries)
			{
				if (entry.Name == s)
				{
					LoadedArchive.Reader.Seek(entry.Offset, SeekOrigin.Begin);

					try
					{
						var model = new ModelFile();
						model.Deserialize(LoadedArchive.Reader);

						var loadedModel = new Model(this, new SmallArchiveWrapper(smallArc, LoadedArchive.Reader), model);
						GameSystems.Add(loadedModel);
						LoadedModel = loadedModel;
					}
					catch (NotSupportedException ex)
					{
						Messages.Add("Failed to load " + s);
					}

					return;
				}
			}
		}

		public void UnloadAllRbm()
		{
			Messages.Clear();
			_shaderErrors.Clear();

			GameSystems.Remove(LoadedModel);
			LoadedModel = null;
			LoadedArchive = default(SmallArchiveWrapper);
		}

		public void CenterCameraOnScene()
		{
			var sceneSphere = new BoundingSphere();
			ModelFile model = LoadedModel.ModelFile;
			var modelBounds = new BoundingBox(
				new Vector3(model.MinX, model.MinY, model.MinZ),
				new Vector3(model.MaxX, model.MaxY, model.MaxZ)
				);

			sceneSphere = BoundingSphere.Merge(sceneSphere, BoundingSphere.FromBox(modelBounds));

			Vector3 delta = (sceneSphere.Center - Camera.Position);

			float distToCenter = sceneSphere.Radius/(float) Math.Sin(Camera.Fov/2);
			Camera.Position = sceneSphere.Center;
			Camera.Position += (Vector3.UnitZ*distToCenter);
			Camera.Position += (Vector3.UnitY*(distToCenter*0.5f));

			delta = (Camera.Position - sceneSphere.Center);

			Camera.CameraRotation = Matrix.RotationYawPitchRoll(
				(float) Math.Atan2(delta.X, delta.Z),
				(float) -Math.Atan2(delta.Y, Math.Sqrt(delta.X*delta.X + delta.Z*delta.Z)),
				0);
		}
	}
}