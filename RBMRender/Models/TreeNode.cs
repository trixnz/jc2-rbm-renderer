using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Gibbed.Avalanche.FileFormats;
using Gibbed.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace RBMRender.Models
{
	public class TreeNode : INotifyPropertyChanged
	{
		private readonly MainViewModel _mainViewModel;
		private bool _isExpanded;
		private bool _isLoaded;
		private bool _isSelected;
		private bool _show = true;

		public TreeNode(MainViewModel mainViewModel)
		{
			ChildrenNodes = new ObservableDictionary<string, TreeNode>();
			MainViewModel.OnLodChanged += FilterLODLevel;

			_mainViewModel = mainViewModel;
		}

		public string Name { get; set; }

		public ObservableDictionary<string, TreeNode> ChildrenNodes { get; set; }

		public string FullPath { get; set; }

		public bool Show
		{
			get { return _show; }
			set
			{
				_show = value;
				OnPropertyChanged();
			}
		}

		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (FullPath != null && !_isLoaded)
				{
					ChildrenNodes.Clear();

					ArchiveTableFile.Entry fileInfo = Archive.ArchiveTable[Name.HashJenkins()];

					Archive.Reader.Seek(fileInfo.Offset, SeekOrigin.Begin);

					// Read in the archive
					// Decompress
					// Deserialize
					// Extract list of files
					// UnloadModel

					var smallArc = new SmallArchiveFile();
					SmallArchive = new SmallArchiveWrapper
					               {
						               SmallArchive = smallArc,
						               Reader = new MemoryStream(ArchiveManager.GetDecompressed(Archive.Reader, fileInfo.Size))
					               };
					if (SmallArchive.Reader != null)
						smallArc.Deserialize(SmallArchive.Reader);

					List<string> newFiles =
						smallArc.Entries.Select(a => a.Name).Where(a => a.EndsWith(".rbm")).OrderBy(a => a).ToList();
					foreach (string f in newFiles)
					{
						HandleBranch<FileNode>(f, null);
					}

					FilterLODLevel();

					_isLoaded = true;
				}
				_isExpanded = value;
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;

				if (!Name.EndsWith("rbm"))
					return;

				Debug.WriteLine("Loading " + Name);
				_mainViewModel.Game.OpenFromArchive(SmallArchive, Name);
			}
		}

		public ArchiveWrapper Archive { get; set; }
		public SmallArchiveWrapper SmallArchive { get; set; }
		public event PropertyChangedEventHandler PropertyChanged;

		~TreeNode()
		{
			MainViewModel.OnLodChanged -= FilterLODLevel;
		}

		public void FilterLODLevel()
		{
			if (_mainViewModel.SelectedLod == null)
				return;

			foreach (TreeNode childrenNode in ChildrenNodes.Values)
			{
				childrenNode.Show = true;

				if (!(childrenNode is FileNode))
					continue;

				if (!childrenNode.Name.Contains(_mainViewModel.SelectedLod))
					childrenNode.Show = false;
			}
		}
		
		public TreeNode HandleBranch<T>(string branch, string fullpath) where T : TreeNode
		{
			TreeNode existingNode = null;
			if (ChildrenNodes.TryGetValue(branch, out existingNode))
				return existingNode;

			var newNode = (T) Activator.CreateInstance(typeof (T), _mainViewModel);
			newNode.Name = branch;
			newNode.FullPath = fullpath;
			newNode.Archive = Archive;
			newNode.SmallArchive = SmallArchive;

			ChildrenNodes.Add(branch, newNode);

			return newNode;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class DummyNode : TreeNode
	{
		public DummyNode(MainViewModel mainViewModel)
			: base(mainViewModel)
		{
		}
	}

	public class FileNode : TreeNode
	{
		public FileNode(MainViewModel mainViewModel) : base(mainViewModel)
		{
		}
	}
}