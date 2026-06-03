namespace ZeroPlay
{
	public class Singleton<T> where T : new()
	{
		private static T ins;

		public static T Ins => default(T);
	}
}
