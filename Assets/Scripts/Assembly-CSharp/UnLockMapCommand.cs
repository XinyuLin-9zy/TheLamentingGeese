using Utage;

public class UnLockMapCommand : AdvCommand
{
	private string name;

	public UnLockMapCommand(StringGridRow row)
		: base(row)
	{
		name = ParseCellOptional<string>(AdvColumnName.Arg1, "");
	}

	public override void DoCommand(AdvEngine engine)
	{
		PlotMapProgressStore.Unlock(name);
	}
}
