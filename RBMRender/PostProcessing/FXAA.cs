using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using RasterizerState = SharpDX.Toolkit.Graphics.RasterizerState;

namespace RBMRender.PostProcessing
{
	public class FXAA
	{
		private readonly GameWorld _game;

		private Effect _fxaaEffect;

		#region FXAA Arguments

		// This effects sub-pixel AA quality and inversely sharpness.
		//   Where N ranges between,
		//     N = 0.50 (default)
		//     N = 0.33 (sharper)
		private float N = 0.40f;

		// Choose the amount of sub-pixel aliasing removal.
		// This can effect sharpness.
		//   1.00 - upper limit (softer)
		//   0.75 - default amount of filtering
		//   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
		//   0.25 - almost off
		//   0.00 - completely off

		// This does not effect PS3, as this needs to be compiled in.
		//   Use FXAA_CONSOLE__PS3_EDGE_SHARPNESS for PS3.
		//   Due to the PS3 being ALU bound,
		//   there are only three safe values here: 2 and 4 and 8.
		//   These options use the shaders ability to a free *|/ by 2|4|8.
		// For all other platforms can be a non-power of two.
		//   8.0 is sharper (default!!!)
		//   4.0 is softer
		//   2.0 is really soft (good only for vector graphics inputs)
		private float consoleEdgeSharpness = 8.0f;

		// This does not effect PS3, as this needs to be compiled in.
		//   Use FXAA_CONSOLE__PS3_EDGE_THRESHOLD for PS3.
		//   Due to the PS3 being ALU bound,
		//   there are only two safe values here: 1/4 and 1/8.
		//   These options use the shaders ability to a free *|/ by 2|4|8.
		// The console setting has a different mapping than the quality setting.
		// Other platforms can use other values.
		//   0.125 leaves less aliasing, but is softer (default!!!)
		//   0.25 leaves more aliasing, and is sharper
		private float consoleEdgeThreshold = 0.125f;

		// Trims the algorithm from processing darks.
		// The console setting has a different mapping than the quality setting.
		// This only applies when FXAA_EARLY_EXIT is 1.
		// This does not apply to PS3, 
		// PS3 was simplified to avoid more shader instructions.
		//   0.06 - faster but more aliasing in darks
		//   0.05 - default
		//   0.04 - slower and less aliasing in darks
		// Special notes when using FXAA_GREEN_AS_LUMA,
		//   Likely want to set this to zero.
		//   As colors that are mostly not-green
		//   will appear very dark in the green channel!
		//   Tune by looking at mostly non-green content,
		//   then start at zero and increase until aliasing is a problem.
		private float consoleEdgeThresholdMin = 0f;
		private float edgeTheshold = 0.166f;

		// Trims the algorithm from processing darks.
		//   0.0833 - upper limit (default, the start of visible unfiltered edges)
		//   0.0625 - high quality (faster)
		//   0.0312 - visible limit (slower)
		// Special notes when using FXAA_GREEN_AS_LUMA,
		//   Likely want to set this to zero.
		//   As colors that are mostly not-green
		//   will appear very dark in the green channel!
		//   Tune by looking at mostly non-green content,
		//   then start at zero and increase until aliasing is a problem.
		private float edgeThesholdMin = 0f;
		private float subPixelAliasingRemoval = 0.75f;

		#endregion

		public FXAA(GameWorld game)
		{
			_game = game;
		}

		public void LoadContent()
		{
			_fxaaEffect = _game.Content.Load<Effect>("fxaa");
		}


		public void DoPostProcessing(RenderTarget2D backTarget)
		{
			ViewportF viewport = _game.GraphicsDevice.Viewport;

			_fxaaEffect.Parameters["InverseViewportSize"].SetValue(new Vector2(1f/viewport.Width, 1f/viewport.Height));
			_fxaaEffect.Parameters["ConsoleSharpness"].SetValue(new Vector4(
				-N/viewport.Width,
				-N/viewport.Height,
				N/viewport.Width,
				N/viewport.Height
				));
			_fxaaEffect.Parameters["ConsoleOpt1"].SetValue(new Vector4(
				-2.0f/viewport.Width,
				-2.0f/viewport.Height,
				2.0f/viewport.Width,
				2.0f/viewport.Height
				));
			_fxaaEffect.Parameters["ConsoleOpt2"].SetValue(new Vector4(
				8.0f/viewport.Width,
				8.0f/viewport.Height,
				-4.0f/viewport.Width,
				-4.0f/viewport.Height
				));
			_fxaaEffect.Parameters["SubPixelAliasingRemoval"].SetValue(subPixelAliasingRemoval);
			_fxaaEffect.Parameters["EdgeThreshold"].SetValue(edgeTheshold);
			_fxaaEffect.Parameters["EdgeThresholdMin"].SetValue(edgeThesholdMin);
			_fxaaEffect.Parameters["ConsoleEdgeSharpness"].SetValue(consoleEdgeSharpness);
			_fxaaEffect.Parameters["ConsoleEdgeThreshold"].SetValue(consoleEdgeThreshold);
			_fxaaEffect.Parameters["ConsoleEdgeThresholdMin"].SetValue(consoleEdgeThresholdMin);

			_fxaaEffect.Parameters["BackTexture"].SetResource(backTarget);
			_fxaaEffect.CurrentTechnique.Passes[0].Apply();

			// Enforce solid rendering, otherwise we're going to draw a great big
			// square across the screen
			var rasterizerState = RasterizerState.New(_game.GraphicsDevice, new RasterizerStateDescription
			{
				FillMode = FillMode.Solid,
				CullMode = CullMode.None,
				IsDepthClipEnabled = false
			});
			_game.GraphicsDevice.SetRasterizerState(rasterizerState);
			DrawQuad(-Vector2.One, Vector2.One);
		}

		private void DrawQuad(Vector2 v1, Vector2 v2)
		{
			using (var batch = new PrimitiveBatch<VertexPositionTexture>(_game.GraphicsDevice))
			{
				batch.Begin();
				batch.DrawIndexed(PrimitiveType.TriangleList, new short[] {0, 1, 2, 2, 3, 0},
					new[]
					{
						new VertexPositionTexture(new Vector3(v2.X, v1.Y, 0), new Vector2(1, 1)),
						new VertexPositionTexture(new Vector3(v1.X, v1.Y, 0), new Vector2(0, 1)),
						new VertexPositionTexture(new Vector3(v1.X, v2.Y, 0), new Vector2(0, 0)),
						new VertexPositionTexture(new Vector3(v2.X, v2.Y, 0), new Vector2(1, 0))
					});
				batch.End();
			}
		}
	}
}