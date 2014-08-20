using System.IO;
using Gibbed.Avalanche.FileFormats;

namespace RBMRender
{
	public class ArchiveWrapper
	{
		public ArchiveWrapper()
		{
		}

		public ArchiveWrapper(ArchiveTableFile archiveTable, FileStream sr)
		{
			ArchiveTable = archiveTable;
			Reader = sr;
		}

		public ArchiveTableFile ArchiveTable { get; set; }
		public Stream Reader { get; set; }

		public byte[] LoadFile(string name)
		{
			// The hash of the filename is used here, rather than the whole
			// directory.
			string fileName = Path.GetFileName(name);
			uint fileNameHash = fileName.HashJenkins();

			if (!ArchiveTable.Contains(fileNameHash))
				return null;

			ArchiveTableFile.Entry generalEntry = ArchiveTable.Get(fileNameHash);
			Reader.Seek(generalEntry.Offset, SeekOrigin.Begin);

			return ArchiveManager.GetDecompressed(Reader, generalEntry.Size);
		}
	}
}