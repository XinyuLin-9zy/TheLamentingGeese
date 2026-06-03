using Utage;
using UnityEngine;
using UI;

public class ScreenTextCommand : AdvCommand
{
	private string content;

	private string textColor;

	private float fadeInTime;

	private float waitTime;

	private float fadeOutTime;

	private float fontSize;

	private int fontIndex;

	private UI_ScreenText screenText;

	private float waitUntil;

	public ScreenTextCommand(StringGridRow row)
		: base(row)
	{
		string arg1 = ParseCellOptional<string>(AdvColumnName.Arg1, "");
		string localizedText = ParseCellLocalizedText();
		content = string.IsNullOrEmpty(localizedText) ? arg1 : localizedText;
		textColor = ParseCellOptional<string>(AdvColumnName.Arg2, "#FFFFFF");
		fadeInTime = ParseCellOptional<float>(AdvColumnName.Arg3, 0.45f);
		waitTime = ParseCellOptional<float>(AdvColumnName.Arg4, 1.4f);
		fadeOutTime = ParseCellOptional<float>(AdvColumnName.Arg5, 0.45f);
		fontSize = ParseCellOptional<float>(AdvColumnName.Arg6, 150f);
		fontIndex = 1;
		int parsedFontIndex;
		if (!string.IsNullOrEmpty(arg1) && int.TryParse(arg1, out parsedFontIndex))
		{
			fontIndex = parsedFontIndex;
		}
	}

	public override void DoCommand(AdvEngine engine)
	{
		CustomCommander commander = null;
		if (engine != null)
		{
			commander = engine.GetComponentInChildren<CustomCommander>(true);
		}
		if (commander == null)
		{
			commander = WrapperFindObject.FindObjectOfTypeIncludeInactive<CustomCommander>();
		}
		if (commander == null) return;

		screenText = commander.ShowScreenText(content, textColor, fadeInTime, waitTime, fadeOutTime, fontSize, fontIndex);
		waitUntil = Time.realtimeSinceStartup + Mathf.Max(0.01f, fadeInTime) + Mathf.Max(0f, waitTime) + Mathf.Max(0.01f, fadeOutTime);
	}

	public override bool Wait(AdvEngine engine)
	{
		if (screenText != null) return screenText.IsPlaying;
		return Time.realtimeSinceStartup < waitUntil;
	}
}
