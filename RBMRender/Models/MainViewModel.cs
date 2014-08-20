using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Gibbed.Avalanche.FileFormats;
using Gibbed.ProjectData;
using Microsoft.Win32;
using RBMRender.RenderBlocks;
using RBMRender.Utilities;

namespace RBMRender.Models
{
	public class BlockEntry
	{
		public BlockEntry()
		{
			Properties = new Dictionary<string, string>();
		}

		public string Name { get; set; }
		public int Vertices { get; set; }
		public Dictionary<string, string> Properties { get; set; }
		public IRenderBlockDrawable Block { get; set; }
	}

	public class MainViewModel : INotifyPropertyChanged
	{
		private readonly GameWorld _game;
		private BlockEntry _selectedBlock;
		private string _selectedLod;
		private bool _showRenderOptions;
		private TreeNode _treeRoot;

		public MainViewModel(GameWorld game)
		{
			_game = game;

			// This is pretty bad.. I should feel bad.. I do. 
			Open = new RelayCommand(e =>
			                        {
				                        var fileDialog = new OpenFileDialog {Filter = "Archive Table (*.tab)|*.tab"};

				                        bool? result = fileDialog.ShowDialog();
				                        if (result == true)
				                        {
					                        PopulateModels(fileDialog.FileName);
				                        }
			                        });

			UnloadModel = new RelayCommand(e => Unload());

			OpenRenderOptions = new RelayCommand(e => { ShowRenderOptions = true; });
			PopulateModels("pc0.tab");
		}

		public RelayCommand Open { get; set; }
		public RelayCommand UnloadModel { get; set; }
		public RelayCommand OpenRenderOptions { get; set; }

		public string SelectedLod
		{
			get { return _selectedLod; }
			set
			{
				_selectedLod = value;

				if (OnLodChanged != null)
					OnLodChanged();
			}
		}

		public bool ShowRenderOptions
		{
			get { return _showRenderOptions; }
			set
			{
				_showRenderOptions = value;
				OnPropertyChanged();
			}
		}

		public ICommand ExpandingCommand { get; set; }

		public GameWorld Game
		{
			get { return _game; }
		}

		public TreeNode TreeRoot
		{
			get { return _treeRoot; }
			set
			{
				_treeRoot = value;
				OnPropertyChanged();
			}
		}

		public BlockEntry SelectedBlock
		{
			get { return _selectedBlock; }
			set
			{
				_selectedBlock = value;

				if (_game.LoadedModel != null)
					_game.LoadedModel.SelectedBlock = _selectedBlock.Block;

				OnPropertyChanged();
			}
		}

		public string SelectedFile { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		public static event Action OnLodChanged;

		public void PopulateModels(string archiveName)
		{
			Unload();

			var treeRoot = new TreeNode(this)
			               {
				               Name = "root!"
			               };

			var archiveWrapper = ArchiveManager.Instance.GetArchive(archiveName);

			var smallArcExtensions = new[]
			                         {
				                         ".eez", ".ee", ".blz", ".flz", ".nlz"
			                         };
			foreach (uint key in archiveWrapper.ArchiveTable.Keys)
			{
				if (!ArchiveManager.Instance.FileNames.Contains(key))
					continue;

				string name = ArchiveManager.Instance.FileNames[key];

				if (smallArcExtensions.Any(ex => ex == Path.GetExtension(name)))
				{
					PopulateSmallArc(treeRoot, archiveWrapper, key, name);
				}
			}

			TreeRoot = treeRoot;
		}

		private void PopulateSmallArc(TreeNode treeRoot, ArchiveWrapper archiveWrapper, uint key, string name)
		{
			string[] components = name.Split(new[] {'/', '\\'});

			TreeNode parent = treeRoot;
			for (int i = 0; i < components.Length; i++)
			{
				string component = components[i];
				string fullpath = null;
				if (i + 1 == components.Length)
					fullpath = name;

				parent = parent.HandleBranch<TreeNode>(component, fullpath);
				parent.Archive = archiveWrapper;

				if (fullpath != null)
				{
					parent.ChildrenNodes.Add("", new DummyNode(this));
				}
			}

			RecursiveSort(treeRoot);
		}

		private void RecursiveSort(TreeNode node)
		{
			foreach (var child in node.ChildrenNodes)
				RecursiveSort(child.Value);

			IOrderedEnumerable<KeyValuePair<string, TreeNode>> myList = from entry in node.ChildrenNodes
				orderby entry.Value.ChildrenNodes.Count descending
				select entry;

			node.ChildrenNodes = new ObservableDictionary<string, TreeNode>();
			foreach (var keyValuePair in myList)
			{
				node.ChildrenNodes.Add(keyValuePair.Key, keyValuePair.Value);
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Unload()
		{
			_game.UnloadAllRbm();
			TreeRoot = null;
			OnLodChanged = null;
		}
	}
}