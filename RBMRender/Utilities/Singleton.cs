namespace RBMRender.Utilities
{
	public class Singleton<T> where T : new()
	{
		public static readonly T Instance = new T();
	}
}