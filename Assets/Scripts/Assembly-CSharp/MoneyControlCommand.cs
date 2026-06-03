using System.Globalization;
using UnityEngine;
using Utage;

public class MoneyControlCommand : AdvCommand
{
	private string action;

	private int count;

	public MoneyControlCommand(StringGridRow row)
		: base(row)
	{
		action = ParseCellOptional<string>(AdvColumnName.Arg1, "");
		count = MoneyCommandUtility.ParseAmount(this, 0, AdvColumnName.Arg2, AdvColumnName.Arg6);
	}

	public override void DoCommand(AdvEngine engine)
	{
		if (engine == null) return;

		string normalized = string.IsNullOrEmpty(action) ? "" : action.Trim().ToLowerInvariant();
		int current = MoneyCommandUtility.GetMoney(engine);
		switch (normalized)
		{
			case "set":
				MoneyCommandUtility.SetMoney(engine, count);
				break;
			case "add":
			case "get":
				MoneyCommandUtility.SetMoney(engine, current + count);
				break;
			case "use":
			case "spend":
			case "cost":
			case "lose":
			case "lost":
			case "sub":
			case "subtract":
				MoneyCommandUtility.SetMoney(engine, Mathf.Max(0, current - count));
				break;
			case "show":
				MoneyCommandUtility.SetBool(engine, "IsMoney", true);
				MoneyCommandUtility.FindDialog()?.ShowMoney(MoneyShowState.Show);
				break;
			case "hide":
				MoneyCommandUtility.SetBool(engine, "IsMoney", false);
				MoneyCommandUtility.FindDialog()?.ShowMoney(MoneyShowState.Hide);
				break;
		}
	}
}

internal static class MoneyCommandUtility
{
	public const string MoneyParamName = "money";

	public static int ParseAmount(AdvCommand command, int defaultValue, params AdvColumnName[] columns)
	{
		foreach (AdvColumnName column in columns)
		{
			if (command.IsEmptyCell(column)) continue;

			string raw;
			if (!command.TryParseCell<string>(column, out raw) || string.IsNullOrWhiteSpace(raw))
			{
				continue;
			}

			int intValue;
			if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue))
			{
				return intValue;
			}

			float floatValue;
			if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue))
			{
				return Mathf.RoundToInt(floatValue);
			}
		}

		return defaultValue;
	}

	public static int GetMoney(AdvEngine engine)
	{
		if (engine == null || engine.Param == null) return 0;

		object value;
		if (!engine.Param.TryGetParameter(MoneyParamName, out value) || value == null)
		{
			return 0;
		}

		if (value is int) return (int)value;
		if (value is float) return Mathf.RoundToInt((float)value);
		return 0;
	}

	public static void SetMoney(AdvEngine engine, int value)
	{
		if (engine == null || engine.Param == null) return;
		engine.Param.TrySetParameter(MoneyParamName, value);
	}

	public static void SetBool(AdvEngine engine, string key, bool value)
	{
		if (engine == null || engine.Param == null) return;
		engine.Param.TrySetParameter(key, value);
	}

	public static UI_DialogMsg FindDialog()
	{
		return WrapperFindObject.FindObjectOfTypeIncludeInactive<UI_DialogMsg>();
	}
}
