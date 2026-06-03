using System;
using System.Collections.Generic;
using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Patterns.Command
{
	public class MacroCommand : Notifier, ICommand, INotifier
	{
		public IList<Func<ICommand>> subcommands;

		protected virtual void InitializeMacroCommand()
		{
		}

		protected void AddSubCommand(Func<ICommand> commandFunc)
		{
		}

		public virtual void Execute(INotification notification)
		{
		}
	}
}
