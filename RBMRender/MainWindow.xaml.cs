using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using RBMRender.Models;

namespace RBMRender
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private readonly GameWorld _game;

		public MainWindow()
		{
			InitializeComponent();

			_game = new GameWorld(this);
			DataContext = new MainViewModel(_game);
		}

		private void Surface_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Surface.Focus();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			_game.Run(Surface);
		}
	}
}