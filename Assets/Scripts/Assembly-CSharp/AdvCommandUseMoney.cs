using Utage;

public class AdvCommandUseMoney : AdvCommand
{
	private int moneyCost;

	public AdvCommandUseMoney(StringGridRow row)
		: base(row)
	{
		moneyCost = MoneyCommandUtility.ParseAmount(this, 0, AdvColumnName.Arg6, AdvColumnName.Arg2, AdvColumnName.Arg1);
	}

	public override void DoCommand(AdvEngine engine)
	{
		MoneyCommandUtility.FindDialog()?.OnUseMoney(moneyCost);
	}
}
