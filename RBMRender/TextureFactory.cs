using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.Avalanche.FileFormats;
using RBMRender.Properties;
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

			if (GeneralSmallArc == null)
				LoadGeneral();
		}

		private static SmallArchiveWrapper GeneralSmallArc { get; set; }

		public GraphicsDevice GraphicsDevice { get; set; }

		private void LoadGeneral()
		{
			ArchiveWrapper archive = ArchiveManager.Instance.GetArchive("pc0.tab");
			GeneralSmallArc = new SmallArchiveWrapper
			                  {
				                  SmallArchive = new SmallArchiveFile(),
				                  Reader = new MemoryStream(archive.LoadFile("general.blz"))
			                  };
			GeneralSmallArc.SmallArchive.Deserialize(GeneralSmallArc.Reader);
		}

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
		/// <param name="smallArchive">The smallArchive containing the file</param>
		/// <param name="entry">The file metadata</param>
		/// <returns>Texture</returns>
		public Texture2D LoadTexture(SmallArchiveWrapper smallArchive, SmallArchiveFile.Entry entry)
		{
			string filename = entry.Name;

			if (FailedTextures.Contains(filename))
				return null;

			Texture2D ret = null;
			if (LoadedTextures.TryGetValue(filename, out ret))
				return ret;

			smallArchive.Reader.Seek(entry.Offset, SeekOrigin.Begin);
			var imageBuf = new byte[entry.Size];
			smallArchive.Reader.Read(imageBuf, 0, imageBuf.Length);

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
		///     Load a texture from general.blz
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

			var entry = GeneralSmallArc.SmallArchive.Entries.FirstOrDefault(e => e.Name == filename);
			if (entry == null)
				return null;

			GeneralSmallArc.Reader.Seek(entry.Offset, SeekOrigin.Begin);
			Texture2D texture = Texture2D.Load(GraphicsDevice, GeneralSmallArc.Reader);
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
}