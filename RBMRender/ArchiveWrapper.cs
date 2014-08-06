using System.IO;
using Gibbed.Avalanche.FileFormats;

namespace RBMRender
{
	public struct ArchiveWrapper
	{
		public ArchiveWrapper(SmallArchiveFile smallArc, FileStream sr) : this()
		{
			SmallArchive = smallArc;
			Reader = sr;
		}

		public SmallArchiveFile SmallArchive { get; set; }
		public Stream Reader { get; set; }
	}
}