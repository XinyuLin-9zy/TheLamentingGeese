using System;
using System.Collections.Concurrent;
using PureMVC.Interfaces;

namespace PureMVC.Core
{
	public class Model : IModel
	{
		protected readonly ConcurrentDictionary<string, IProxy> proxyMap;

		protected static IModel instance;

		protected const string Singleton_MSG = "Model Singleton already constructed!";

		protected virtual void InitializeModel()
		{
		}

		public static IModel GetInstance(Func<IModel> modelFunc)
		{
			return null;
		}

		public virtual void RegisterProxy(IProxy proxy)
		{
		}

		public virtual IProxy RetrieveProxy(string proxyName)
		{
			return null;
		}

		public virtual IProxy RemoveProxy(string proxyName)
		{
			return null;
		}

		public virtual bool HasProxy(string proxyName)
		{
			return false;
		}
	}
}
