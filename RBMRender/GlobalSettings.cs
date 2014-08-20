using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RBMRender
{
	public class GlobalSettings : INotifyPropertyChanged
	{
		private static GlobalSettings _instance;
		private bool _normalDebugging;
		private float _normalLength;
		private bool _wireframeEnabled;

		public GlobalSettings()
		{
			NormalLength = 0.1f;
		}

		public static GlobalSettings Instance
		{
			get { return _instance ?? (_instance = new GlobalSettings()); }
		}

		public bool WireframeEnabled
		{
			get { return _wireframeEnabled; }
			set
			{
				_wireframeEnabled = value;

				OnPropertyChanged();
			}
		}

		public bool NormalDebugging
		{
			get { return _normalDebugging; }
			set
			{
				_normalDebugging = value;
				OnPropertyChanged();
			}
		}

		public float NormalLength
		{
			get { return _normalLength; }
			set
			{
				_normalLength = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}