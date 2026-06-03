using Utage;

public class AdvCommandGetMoney : AdvCommand
{
	private int moneyGet;

	public AdvCommandGetMoney(StringGridRow row)
		: base(row)
	{
		moneyGet = MoneyCommandUtility.ParseAmount(this, 0, AdvColumnName.Arg6, AdvColumnName.Arg2, AdvColumnName.Arg1);
	}

	public override void DoCommand(AdvEngine engine)
	{
		MoneyCommandUtility.FindDialog()?.OnGetMoney(moneyGet);
	}
}
