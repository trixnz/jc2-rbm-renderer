using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace RBMRender
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private GameWorld _game;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Surface_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Surface.Focus();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			_game = new GameWorld(this);
			_game.Run(Surface);
		}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var fileDialog = new OpenFileDialog();

			var result = fileDialog.ShowDialog();
			if (result == true)
			{
				_game.OpenArchive(fileDialog.FileName);
			}
		}

		private void lstModels_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			_game.UnloadAllRbm();
			foreach (var selectedItem in lstModels.SelectedItems)
			{
				_game.LoadRbm(selectedItem as string);
			}
		}
	}
}