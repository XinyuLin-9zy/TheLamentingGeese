using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Patterns.Proxy
{
	public class Proxy : Notifier, IProxy, INotifier
	{
		public static string NAME;

		public string ProxyName { get; protected set; }

		public object Data { get; set; }

		public Proxy(string proxyName, object data = null)
		{
		}

		public virtual void OnRegister()
		{
		}

		public virtual void OnRemove()
		{
		}
	}
}
