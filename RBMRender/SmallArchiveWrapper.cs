using System.IO;
using Gibbed.Avalanche.FileFormats;

namespace RBMRender
{
	public class SmallArchiveWrapper
	{
		public SmallArchiveWrapper()
		{
			
		}

		public SmallArchiveWrapper(SmallArchiveFile smallArc, Stream sr)
		{
			SmallArchive = smallArc;
			Reader = sr;
		}

		public SmallArchiveFile SmallArchive { get; set; }
		public Stream Reader { get; set; }
	}
}