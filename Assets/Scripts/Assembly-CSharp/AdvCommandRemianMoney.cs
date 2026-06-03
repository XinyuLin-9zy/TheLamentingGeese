using Utage;

public class AdvCommandRemianMoney : AdvCommand
{
	private int moneyReamin;

	public AdvCommandRemianMoney(StringGridRow row)
		: base(row)
	{
		moneyReamin = MoneyCommandUtility.ParseAmount(this, 0, AdvColumnName.Arg6, AdvColumnName.Arg2, AdvColumnName.Arg1);
	}

	public override void DoCommand(AdvEngine engine)
	{
		UI_DialogMsg dialog = MoneyCommandUtility.FindDialog();
		if (dialog == null) return;

		dialog.ShowMoney(MoneyShowState.Show);
		if (moneyReamin > 0)
		{
			dialog.ShowMoney(MoneyShowState.Once, MoneyUseType.None, moneyReamin);
		}
	}
}
