using System.Collections.Generic;
using System.IO;
using Gibbed.Avalanche.FileFormats;
using SharpDX.Toolkit.Graphics;

namespace RBMRender
{
	public class TextureFactory
	{
		public delegate void LoadFailedDlg(string texture);

		private static readonly Dictionary<string, Texture2D> LoadedTextures = new Dictionary<string, Texture2D>();
		private static readonly HashSet<string> FailedTextures = new HashSet<string>();

		public TextureFactory(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
		}

		public GraphicsDevice GraphicsDevice { get; set; }

		public event LoadFailedDlg LoadFailed;

		protected virtual void OnLoadFailed(string texture)
		{
			LoadFailedDlg handler = LoadFailed;

			if (handler != null)
				handler(texture);
		}

		/// <summary>
		///     Load a texture from a SmallArc
		/// </summary>
		/// <param name="archive">The archive containing the SmallArc</param>
		/// <param name="entry">The file metadata</param>
		/// <returns>Texture</returns>
		public Texture2D LoadTexture(ArchiveWrapper archive, SmallArchiveFile.Entry entry)
		{
			string filename = entry.Name;

			if (FailedTextures.Contains(filename))
				return null;

			Texture2D ret = null;
			if (LoadedTextures.TryGetValue(filename, out ret))
				return ret;

			archive.Reader.Seek(entry.Offset, SeekOrigin.Begin);
			var imageBuf = new byte[entry.Size];
			archive.Reader.Read(imageBuf, 0, imageBuf.Length);

			using (var ms = new MemoryStream(imageBuf))
			{
				Texture2D texture = Texture2D.Load(GraphicsDevice, ms);

				// Well.. it failed to load.
				if (texture == null)
				{
					OnLoadFailed(entry.Name);
					FailedTextures.Add(entry.Name);

					return null;
				}

				LoadedTextures[entry.Name] = texture;
				return texture;
			}
		}

		/// <summary>
		///     Load a texture from the filesystem
		/// </summary>
		/// <param name="filename">Filename of the texture to be loaded</param>
		/// <returns>Texture</returns>
		public Texture2D LoadTexture(string filename)
		{
			if (FailedTextures.Contains(filename))
				return null;

			Texture2D ret = null;

			if (LoadedTextures.TryGetValue(filename, out ret))
				return ret;

			// Path to an unpacked general.blz for texture lookups (Temporary)
			// Should instead load up general.blz and index the textures so they
			// can be loaded on demand. Loading them all would take too much time.
			var searchPath = new[]
			                 {
				                 Properties.Settings.Default.UnpackedGeneral
			                 };

			foreach (string path in searchPath)
			{
				string fullPath = Path.Combine(path, filename);

				if (File.Exists(fullPath))
				{
					Texture2D texture = Texture2D.Load(GraphicsDevice, fullPath);
					// Well.. it failed to load.
					if (texture == null)
					{
						OnLoadFailed(filename);
						FailedTextures.Add(filename);

						return null;
					}

					LoadedTextures[filename] = texture;

					return texture;
				}
			}

			return null;
		}
	}
}