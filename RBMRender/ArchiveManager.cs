using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Avalanche.FileFormats;
using Gibbed.IO;
using Gibbed.ProjectData;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using RBMRender.Utilities;

namespace RBMRender
{
	public class ArchiveManager : Singleton<ArchiveManager>
	{
		private readonly Dictionary<string, ArchiveWrapper> _loadedArchives =
			new Dictionary<string, ArchiveWrapper>(StringComparer.OrdinalIgnoreCase);

		private Manager _manager;

		public ArchiveManager()
		{
			_manager = Manager.Load("Just Cause 2");
			FileNames = _manager.ActiveProject.LoadListsFileNames();

			ArchivePath = Path.Combine(_manager.ActiveProject.InstallPath, "archives_win32");
		}

		public HashList<uint> FileNames { get; private set; }

		public string ArchivePath { get; set; }

		public ArchiveWrapper GetArchive(string archiveName)
		{
			if (Path.GetExtension(archiveName) != ".tab")
				throw new Exception("Expected .tab file when opening archive");

			ArchiveWrapper archive;
			if (_loadedArchives.TryGetValue(archiveName, out archive))
				return archive;

			string archivePath = Path.Combine(ArchivePath, archiveName);

			var archiveWrapper = new ArchiveWrapper();
			using (FileStream fs = File.OpenRead(archivePath))
			{
				archiveWrapper.ArchiveTable = new ArchiveTableFile();
				archiveWrapper.ArchiveTable.Deserialize(fs);
			}
			archiveWrapper.Reader = File.OpenRead(Path.ChangeExtension(archivePath, ".arc"));

			_loadedArchives.Add(archiveName, archiveWrapper);

			return archiveWrapper;
		}

		public static byte[] GetDecompressed(Stream input, uint size)
		{
			bool decompress = input.ReadValueU8() == 0x78;

			input.Seek(-1, SeekOrigin.Current);

			if (decompress == false)
			{
				var outData = new byte[size];
				input.Read(outData, 0, outData.Length);

				return outData;
			}

			var decompressed = new MemoryStream();

			//var zlib = new ZlibStream(input, CompressionMode.Decompress);
			var zlib = new InflaterInputStream(input)
			           {
				           IsStreamOwner = false
			           };

			var buffer = new byte[0x4000];
			while (true)
			{
				int read = zlib.Read(buffer, 0, buffer.Length);
				if (read < 0)
				{
					throw new InvalidOperationException("zlib error");
				}

				if (read == 0)
				{
					break;
				}

				decompressed.Write(buffer, 0, read);
			}

			zlib.Close();
			decompressed.Position = 0;

			return decompressed.ToArray();
		}
	}
}