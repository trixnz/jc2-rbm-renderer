using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;

namespace RBMRender.Flyouts
{
	/// <summary>
	///     Interaction logic for RenderOptions.xaml
	/// </summary>
	public partial class RenderOptions : Flyout
	{
		public RenderOptions()
		{
			DataContext = GlobalSettings.Instance;
			InitializeComponent();
		}
	}
}