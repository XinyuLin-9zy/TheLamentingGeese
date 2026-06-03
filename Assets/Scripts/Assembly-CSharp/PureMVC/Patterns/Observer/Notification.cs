using PureMVC.Interfaces;

namespace PureMVC.Patterns.Observer
{
	public class Notification : INotification
	{
		public string Name { get; }

		public object Body { get; set; }

		public object Body1 { get; set; }

		public object Body2 { get; set; }

		public object Body3 { get; set; }

		public object Body4 { get; set; }

		public object Body5 { get; set; }

		public Notification(string name)
		{
		}

		public Notification(string name, object body)
		{
		}

		public Notification(string name, object body, object body1)
		{
		}

		public Notification(string name, object body, object body1, object body2)
		{
		}

		public Notification(string name, object body, object body1, object body2, object body3)
		{
		}

		public Notification(string name, object body, object body1, object body2, object body3, object body4)
		{
		}

		public Notification(string name, object body, object body1, object body2, object body3, object body4, object body5)
		{
		}

		public override string ToString()
		{
			return null;
		}
	}
}
